using Aspose.Cells;
using ExcelVideoLablerAPI.Common.Constants;
using ExcelVideoLablerAPI.Common.Responses;
using ExcelVideoLablerAPI.Enums;
using ExcelVideoLablerAPI.Models;
using ExcelVideoLablerAPI.Repositories;
using ExcelVideoLablerAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExcelVideoLablerAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class VideoController : ControllerBase
    {
        private readonly IVideoInfoRepository videoInfoRepository;
        private readonly ConfigService configService;
        private readonly VideoExcelService videoExcelService;
        private readonly IConfigRepository configRepository;
        private readonly DownloadVideoService downloadVideoService;
        private readonly IWebHostEnvironment env;

        private static readonly SemaphoreSlim _semaphore = new(1, 1);
        private static volatile bool _isDownloading;
        // private static volatile bool _cancelRequested;
        private static CancellationTokenSource _cts = new CancellationTokenSource();

        public VideoController(IVideoInfoRepository videoInfoRepository,
            ConfigService configService,
            VideoExcelService videoExcelService,
            IConfigRepository configRepository,
            DownloadVideoService downloadVideoService,
            IWebHostEnvironment env)
        {
            this.videoInfoRepository = videoInfoRepository;
            this.configService = configService;
            this.videoExcelService = videoExcelService;
            this.configRepository = configRepository;
            this.downloadVideoService = downloadVideoService;
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
                    Message = "File không tồn tại."
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
                    Message = "File không tồn tại."
                });
            }

            using Workbook workbook = new Workbook(filePath);

            // Lấy sheet đầu tiên
            using Worksheet sheet = workbook.Worksheets[ConfigService.Config.SheetIndex];
            var listVideo = videoExcelService.ReadDataFromSheet(sheet);
            var listNewVideo = await videoInfoRepository.AddRange(listVideo);

            if (listVideo.Count == 0)
            {
                var newConfig = new Config()
                {
                    SheetIndex = ConfigService.Config.SheetIndex + 1,
                };
                await configRepository.Update(newConfig);
                configService.Update(newConfig);
                return BadRequest(new ApiResponse<List<VideoInfo>>()
                {
                    Data = listNewVideo,
                    Message = "Hết dữ liệu. Sang sheet tiếp theo."
                });
            }

            if (listNewVideo.Count < VideoConstants.TotalVideoToDownload)
            {
                var newConfig = new Config()
                {
                    SheetIndex = ConfigService.Config.SheetIndex + 1,
                    RowIndex = 1
                };
                await configRepository.Update(newConfig);
                configService.Update(newConfig);
            }
            else
            {
                var newConfig = new Config()
                {
                    RowIndex = ConfigService.Config.RowIndex + listNewVideo.Count
                };
                await configRepository.Update(newConfig);
                configService.Update(newConfig);
            }

            return Ok(new ApiResponse<object>()
            {
                Data = new
                {
                    ListNewVideo = listNewVideo.Take(10).ToList(),
                    Total = listNewVideo.Count
                },
                Message = ""
            });
        }

        [HttpPost]
        public async Task<IActionResult> StartDownload([FromQuery] int maxParallel = 3)
        {
            if (_isDownloading)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Message = "Đang tải video. Vui lòng đợi hoàn tất hoặc dừng quá trình hiện tại."
                });
            }

            await _semaphore.WaitAsync();
            _isDownloading = true;
            _cts = new CancellationTokenSource(); // Khởi tạo mới mỗi lần
            var token = _cts.Token;

            try
            {
                var listVideo = await videoInfoRepository.GetAll(VideoStatus.Pending);
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
                            video.VideoStatus = check ? VideoStatus.Downloaded : VideoStatus.ErrorLink;
                        }
                        catch (OperationCanceledException)
                        {
                            video.VideoStatus = VideoStatus.Pending; // Hoặc trạng thái riêng
                        }
                        catch
                        {
                            video.VideoStatus = VideoStatus.ErrorLink;
                        }
                        finally
                        {
                            throttler.Release();
                        }
                    }, token);

                    tasks.Add(task);
                }

                await Task.WhenAll(tasks);
                await videoInfoRepository.UpdateRange(listVideo);

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