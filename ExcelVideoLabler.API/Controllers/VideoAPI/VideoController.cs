using Aspose.Cells;
using ExcelVideoLabler.API.Constants;
using ExcelVideoLabler.API.Controllers.Common;
using ExcelVideoLabler.API.Hubs;
using ExcelVideoLabler.API.Models;
using ExcelVideoLabler.API.Services;
using ExcelVideoLabler.Domain.Entities;
using ExcelVideoLabler.Domain.Enums;
using ExcelVideoLabler.Infrastructure.Repositories.ConfigRepository;
using ExcelVideoLabler.Infrastructure.Repositories.Models;
using ExcelVideoLabler.Infrastructure.Repositories.VideoInfoRepository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace ExcelVideoLabler.API.Controllers.VideoAPI
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class VideoController : ControllerBase
    {
        private readonly ConfigService configService;
        private readonly VideoExcelService videoExcelService;
        private readonly VideoService videoService;
        private readonly IVideoInfoQueryRepository videoInfoQueryRepository;
        private readonly IVideoInfoCommandRepository videoInfoCommandRepository;
        private readonly IConfigCommandRepository configCommandRepository;
        private readonly VideoDownloadHubService videoDownloadHubService;
        private readonly IServiceScopeFactory scopeFactory;
        private readonly IWebHostEnvironment env;

        private static readonly SemaphoreSlim _semaphore = new(1, 1);
        private static volatile bool _isDownloading;
        private static volatile bool _cancelRequested;
        private static CancellationTokenSource cts = new();

        public VideoController(
            ConfigService configService,
            VideoExcelService videoExcelService,
            VideoService videoService,
            IVideoInfoQueryRepository videoInfoQueryRepository,
            IVideoInfoCommandRepository  videoInfoCommandRepository,
            IConfigCommandRepository configCommandRepository,
            VideoDownloadHubService videoDownloadHubService,
            IServiceScopeFactory scopeFactory,
            IWebHostEnvironment env)
        {
            this.configService = configService;
            this.videoExcelService = videoExcelService;
            this.videoService = videoService;
            this.videoInfoQueryRepository = videoInfoQueryRepository;
            this.videoInfoCommandRepository = videoInfoCommandRepository;
            this.configCommandRepository = configCommandRepository;
            this.videoDownloadHubService = videoDownloadHubService;
            this.scopeFactory = scopeFactory;
            this.env = env;
        }

        [HttpGet]
        public async Task<IActionResult> GetRemainingVideos()
        {
            if (string.IsNullOrEmpty(ConfigService.Config.ExceFileName))
            {
                return Ok(new ApiResponse<object>()
                {
                    Data = new
                    {
                        Total = 0,
                        SheetName = "",
                        StartRowInSheet = -1,
                    },
                    Message = "File không tồn tại. Vui lòng upload file."
                });
            }
            
            string filePath = Path.Combine(env.WebRootPath, FolderConstants.ExcelFolder,
                ConfigService.Config.ExceFileName);
            if (!System.IO.File.Exists(filePath))
            {
                return Ok(new ApiResponse<object>()
                {
                    Data = new
                    {
                        Total = 0,
                        SheetName = "",
                        StartRowInSheet = -1,
                    },
                    Message = "File không tồn tại. Vui lòng upload file."
                });
            }
            
            using Workbook workbook = new Workbook(filePath);
            using Worksheet sheet = workbook.Worksheets[ConfigService.Config.SheetIndex];
            QueryOptionsBuilder<VideoInfo> queryOptionsBuilder = new();
            queryOptionsBuilder.Where(x => x.VideoStatus == VideoStatus.Pending);
            var r = await videoInfoQueryRepository.FilterAsync(queryOptionsBuilder.Build());
            
            return Ok(new ApiResponse<object>()
            {
                Data = new
                {
                    ListVideo = r.Take(5).ToList(),
                    Total = r.Count,
                    SheetName = sheet.Name,
                    StartRowInSheet = ConfigService.Config.RowIndex,
                },
                Message = "File không tồn tại. Vui lòng upload file."
            });
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
                return Ok (new ApiResponse<object>
                {
                    Message = "Quá trình tải video đang thực hiện."
                });
            }
            
            cts = new CancellationTokenSource();
            await _semaphore.WaitAsync();
            _isDownloading = true;
            _cancelRequested = false;
            var token = cts.Token;

            try
            {
                var queryOptionBuilder = new QueryOptionsBuilder<VideoInfo>();
                queryOptionBuilder.Where(x => x.VideoStatus == VideoStatus.Pending);
                var listVideo = await videoInfoQueryRepository.FilterAsync(queryOptionBuilder.Build());
                int total = listVideo.Count;

                if (total == 0)
                {
                    _isDownloading = false;
                    cts.Cancel();
                    return Ok(new ApiResponse<object>
                    {
                        Data = new
                        {
                            Total = total,
                        },
                        Message = "Tất cả video đã được tải."
                    });
                }
                
                int maxConcurrentDownloads = 3; // Số video tải đồng thời
                
                _ = Task.Run(async () =>
                {
                    using var scope = scopeFactory.CreateScope();
                    var _VideoService = scope.ServiceProvider.GetRequiredService<VideoService>();
                    var _videoInfoCommandRepository = scope.ServiceProvider.GetRequiredService<IVideoInfoCommandRepository>();
                    var _videoDownloadHubService = scope.ServiceProvider.GetRequiredService<VideoDownloadHubService>();
                    
                    var semaphoreSlim = new SemaphoreSlim(maxConcurrentDownloads);
                    var downloadTasks = new List<Task>();
                    int totalSuccess = 0;
                    int totalFailed = 0;

                    try
                    {
                        for (int i = 0; i < total; i++)
                        {
                            if (_cancelRequested || token.IsCancellationRequested)
                                break;

                            await semaphoreSlim.WaitAsync(token);

                            int index = i; // Local copy for closure
                            var task = Task.Run(async () =>
                            {
                                try
                                {
                                    var video = listVideo[index];
                                    bool check = await _VideoService.DownloadVideo(video.Link, video.TransID, token);

                                    if (!check)
                                    {
                                        video.VideoStatus = VideoStatus.ErrorLink;
                                        Interlocked.Increment(ref totalFailed);
                                        await _videoDownloadHubService.RecieveTotalVideo(new ResultDownloadVideo()
                                        {
                                            IsSuccess = false,
                                            TotalDownloadsSuccess = 0,
                                            TotalDownloadsFailed = 1,
                                            Link = video.Link,
                                            TransId = video.TransID,
                                        });
                                    }
                                    else
                                    {
                                        video.VideoStatus = VideoStatus.Downloaded;
                                        Interlocked.Increment(ref totalSuccess);
                                        await _videoDownloadHubService.RecieveTotalVideo(new ResultDownloadVideo()
                                        {
                                            IsSuccess = true,
                                            TotalDownloadsSuccess = 1,
                                            TotalDownloadsFailed = 0,
                                            Link = video.Link,
                                            TransId = video.TransID,
                                        });
                                    }
                                    
                                    await _videoInfoCommandRepository.UpdateAsync(video);
                                    await _VideoService.SaveFileVideoInfo(video);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Lỗi khi tải video: {ex.Message}");
                                }
                                finally
                                {
                                    semaphoreSlim.Release();
                                }
                            }, token);

                            downloadTasks.Add(task);
                        }
                        await Task.WhenAll(downloadTasks);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Lỗi trong background download: {ex.Message}");
                    }
                    finally
                    {
                        await _videoDownloadHubService.SendDownloadFinish(true);
                        _isDownloading = false;
                        _semaphore.Release();
                    }
                });

                return Ok(new ApiResponse<object>
                {
                    Message = "Đang tải video."
                });
            }
            catch (Exception ex)
            {
                _isDownloading = false;
                _semaphore.Release();
                return StatusCode(500, new ApiResponse<object> { Message = $"Lỗi: {ex.Message}" });
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

            cts.Cancel(); // Huỷ mọi token đang dùng

            return Ok(new ApiResponse<object>
            {
                Message = "Dừng tải video."
            });
        }
             
        #region private methods

        private async Task UpdateConfig(Config newConfig)
        {
            configService.Update(newConfig);
            await configCommandRepository.UpdateAsync(ConfigService.Config);
        }
        
        #endregion

    }
}