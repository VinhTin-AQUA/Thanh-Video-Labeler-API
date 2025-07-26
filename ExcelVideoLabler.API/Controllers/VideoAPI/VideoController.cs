using Aspose.Cells;
using ExcelVideoLabler.API.Constants;
using ExcelVideoLabler.API.Controllers.Common;
using ExcelVideoLabler.API.Hubs;
using ExcelVideoLabler.API.Services;
using ExcelVideoLabler.Domain.Entities;
using ExcelVideoLabler.Domain.Enums;
using ExcelVideoLabler.Infrastructure.Repositories.ConfigRepository;
using ExcelVideoLabler.Infrastructure.Repositories.Models;
using ExcelVideoLabler.Infrastructure.Repositories.VideoInfoRepository;
using Microsoft.AspNetCore.Mvc;

namespace ExcelVideoLabler.API.Controllers.VideoAPI
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class VideoController : ControllerBase
    {
        private readonly ConfigService configService;
        private readonly VideoExcelService videoExcelService;
        private readonly DownloadVideoService downloadVideoService;
        private readonly IVideoInfoQueryRepository videoInfoQueryRepository;
        private readonly IVideoInfoCommandRepository videoInfoCommandRepository;
        private readonly IConfigCommandRepository configCommandRepository;
        private readonly VideoDowloadHubService videoDowloadHubService;
        private readonly IWebHostEnvironment env;

        private static readonly SemaphoreSlim _semaphore = new(1, 1);
        private static volatile bool _isDownloading;
        // private static volatile bool _cancelRequested;
        private static CancellationTokenSource _cts = new();

        public VideoController(
            ConfigService configService,
            VideoExcelService videoExcelService,
            DownloadVideoService downloadVideoService,
            IVideoInfoQueryRepository videoInfoQueryRepository,
            IVideoInfoCommandRepository  videoInfoCommandRepository,
            IConfigCommandRepository configCommandRepository,
            VideoDowloadHubService videoDowloadHubService,
            IWebHostEnvironment env)
        {
            this.configService = configService;
            this.videoExcelService = videoExcelService;
            this.downloadVideoService = downloadVideoService;
            this.videoInfoQueryRepository = videoInfoQueryRepository;
            this.videoInfoCommandRepository = videoInfoCommandRepository;
            this.configCommandRepository = configCommandRepository;
            this.videoDowloadHubService = videoDowloadHubService;
            this.env = env;
        }

        [HttpPost]
        public async Task<IActionResult> InitDownloadVideo()
        {
            if (string.IsNullOrEmpty(ConfigService.Config.ExceFileName))
            {
                return BadRequest(new ApiResponse<object>()
                {
                    Data = null,
                    Message = "File không tồn tại. Vui lòng upload file."
                });
            }

            // Load file Excel
            string filePath = Path.Combine(env.WebRootPath, FolderConstants.ExcelFolder,
                ConfigService.Config.ExceFileName);
            if (!System.IO.File.Exists(filePath))
            {
                return BadRequest(new ApiResponse<object>()
                {
                    Data = null,
                    Message = "File không tồn tại. Vui lòng upload file."
                });
            }

            var exists = await videoInfoQueryRepository.ExistsAsync();
            if (exists)
            {
                return BadRequest(new ApiResponse<object>()
                {
                    Data = null,
                    Message = "Vui lòng hoàn tất quá trình gán nhãn và xuất file excel."
                });
            }

            using Workbook workbook = new Workbook(filePath);

            // Lấy sheet đầu tiên
            using Worksheet sheet = workbook.Worksheets[ConfigService.Config.SheetIndex];
            var listVideo = videoExcelService.ReadDataFromSheet(sheet);
            var listNewVideo = await videoInfoCommandRepository.AddRangeAsync(listVideo);

            if (listVideo.Count == 0)
            {
                var newConfig = new Config()
                {
                    SheetIndex = ConfigService.Config.SheetIndex + 1,
                };
                
                await UpdateConfig(newConfig);
                return BadRequest(new ApiResponse<List<VideoInfo>>()
                {
                    Data = listNewVideo.ToList(),
                    Message = "Hết dữ liệu. Sang sheet tiếp theo. Vui lòng bấm Init lần nữa."
                });
            }

            if (listNewVideo.Count < VideoConstants.TotalVideoToDownload)
            {
                var newConfig = new Config()
                {
                    SheetIndex = ConfigService.Config.SheetIndex + 1,
                    RowIndex = 1
                };
                await UpdateConfig(newConfig);
            }
            else
            {
                var newConfig = new Config()
                {
                    RowIndex = ConfigService.Config.RowIndex + listNewVideo.Count
                };
                await UpdateConfig(newConfig);
            }

            return Ok(new ApiResponse<object>()
            {
                Data = new
                {
                    ListNewVideo = listNewVideo.Take(5).ToList(),
                    Total = listNewVideo.Count,
                    SheetName = sheet.Name,
                    StartRowInSheet = ConfigService.Config.RowIndex,
                },
                Message = "Init hoàn tất."
            });
        }

        [HttpPost]
        public async Task<IActionResult> StartDownload()
        {
            if (_isDownloading)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Message = "Đang tải video. Vui lòng đợi hoàn tất hoặc dừng quá trình hiện tại."
                });
            }

            int maxParallel = 2;
            await _semaphore.WaitAsync();
            _isDownloading = true;
            _cts = new CancellationTokenSource(); // Khởi tạo mới mỗi lần
            var token = _cts.Token;

            try
            {
                var queryOptionBuilder = new QueryOptionsBuilder<VideoInfo>();
                queryOptionBuilder.Where(x => x.VideoStatus == VideoStatus.Pending);
                var listVideo = await videoInfoQueryRepository.FilterAsync(queryOptionBuilder.Build());
                var throttler = new SemaphoreSlim(maxParallel);
                var tasks = new List<Task>();

                foreach (var video in listVideo)
                {
                    await throttler.WaitAsync(token); // Dừng nếu token bị huỷ

                    var task = Task.Run(async () =>
                    {
                        try
                        {
                            if (token.IsCancellationRequested) return;

                            bool check =
                                await downloadVideoService.StartDownloadingAsync(video.Link, video.TransID, token);

                            if (check)
                            {
                                video.VideoStatus = VideoStatus.Downloaded;
                                await videoDowloadHubService.RecieveTotalVideo(1, 0);
                            }
                            else
                            {
                                video.VideoStatus = VideoStatus.ErrorLink;
                                await videoDowloadHubService.RecieveTotalVideo(0, 1);
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            await videoDowloadHubService.RecieveTotalVideo(0, 1);
                            video.VideoStatus = VideoStatus.Pending; // Hoặc trạng thái riêng
                        }
                        finally
                        {
                            throttler.Release();
                        }
                    }, token);

                    tasks.Add(task);
                }

                await Task.WhenAll(tasks);
                await videoInfoCommandRepository.UpdateRangeAsync(listVideo);

                return Ok(new ApiResponse<object>()
                {
                    Message = "Tải xuống hoàn tất hoặc đã bị dừng."
                });
            }
            catch (OperationCanceledException)
            {
                return Ok(new ApiResponse<object> { Message = "Quá trình tải đã bị huỷ." });
            }
            finally
            {
                _isDownloading = false;
                _semaphore.Release();
            }
        }

        [HttpPost]
        public IActionResult StopDownload()
        {
            if (!_isDownloading)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Message = "Không có tiến trình tải nào đang diễn ra."
                });
            }

            _cts.Cancel(); // Huỷ mọi token đang dùng

            return Ok(new ApiResponse<object>
            {
                Message = "Đã gửi yêu cầu dừng tải video."
            });
        }

        
        #region private methods

        private async Task UpdateConfig(Config newConfig)
        {
            configService.Update(newConfig);
            await configCommandRepository.UpdateAsync(ConfigService.Config);
        }
        
        #endregion
        
        // [HttpPost]
        // public async Task<IActionResult> StartDownload()
        // {
        //     if (_isDownloading)
        //     {
        //         return BadRequest(new ApiResponse<object>
        //         {
        //             Message = "Đang tải video. Vui lòng đợi hoàn tất hoặc dừng quá trình hiện tại."
        //         });
        //     }
        //     
        //     await _semaphore.WaitAsync();
        //     _isDownloading = true;
        //     _cancelRequested = false;
        //     try
        //     {
        //         var listVideo = await videoInfoRepository.GetAll(VideoStatus.Pending);
        //         int total =  listVideo.Count;
        //         
        //         for(int i = 0; i < total; i++)
        //         {
        //             if (_cancelRequested)
        //             {
        //                 return Ok(new ApiResponse<object>
        //                 {
        //                     Message = "Đã dừng quá trình tải video."
        //                 });
        //             }
        //
        //             bool check = await downloadVideoService.StartDownloadingAsync(listVideo[i].Link, listVideo[i].TransID);
        //
        //             if (!check)
        //             {
        //                 listVideo[i].VideoStatus = VideoStatus.ErrorLink;
        //                 continue;
        //             }
        //
        //             listVideo[i].VideoStatus = VideoStatus.Downloaded;
        //         }
        //         
        //         await videoInfoRepository.UpdateRange(listVideo);
        //         return Ok(new ApiResponse<object>()
        //         {
        //             Data = null,
        //             Message = "Tải xuống hoàn tất."
        //         });
        //     }
        //     catch (Exception ex)
        //     {
        //         throw new Exception(ex.Message);
        //     } 
        //     finally
        //     {
        //         _isDownloading = false;
        //         _semaphore.Release();
        //     }
        // }
    }
}