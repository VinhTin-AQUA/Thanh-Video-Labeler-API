using Aspose.Cells;
using ExcelVideoLabeler.API.Constants;
using ExcelVideoLabeler.API.Controllers.Common;
using ExcelVideoLabeler.API.Controllers.VideoAPI.Payload;
using ExcelVideoLabeler.API.Hubs;
using ExcelVideoLabeler.API.Models;
using ExcelVideoLabeler.API.Services;
using ExcelVideoLabeler.Domain.Entities;
using ExcelVideoLabeler.Domain.Enums;
using ExcelVideoLabeler.Infrastructure.Repositories.ConfigRepository;
using ExcelVideoLabeler.Infrastructure.Repositories.Models;
using ExcelVideoLabeler.Infrastructure.Repositories.VideoInfoRepository;
using Microsoft.AspNetCore.Mvc;

namespace ExcelVideoLabeler.API.Controllers.VideoAPI
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class VideoController : ControllerBase
    {
        private readonly ConfigService configService;
        private readonly VideoExcelService videoExcelService;
        private readonly IVideoInfoQueryRepository videoInfoQueryRepository;
        private readonly IVideoInfoCommandRepository videoInfoCommandRepository;
        private readonly IConfigCommandRepository configCommandRepository;
        private readonly IServiceScopeFactory scopeFactory;
        private readonly IWebHostEnvironment env;

        private static readonly SemaphoreSlim _semaphore = new(1, 1);
        private static volatile bool _isDownloading;
        private static volatile bool _cancelRequested;
        private static CancellationTokenSource cts = new();

        public VideoController(
            ConfigService configService,
            VideoExcelService videoExcelService,
            IVideoInfoQueryRepository videoInfoQueryRepository,
            IVideoInfoCommandRepository  videoInfoCommandRepository,
            IConfigCommandRepository configCommandRepository,
            IServiceScopeFactory scopeFactory,
            IWebHostEnvironment env)
        {
            this.configService = configService;
            this.videoExcelService = videoExcelService;
            this.videoInfoQueryRepository = videoInfoQueryRepository;
            this.videoInfoCommandRepository = videoInfoCommandRepository;
            this.configCommandRepository = configCommandRepository;
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
                                        await _VideoService.SaveFileVideoInfo(video);
                                    }
                                    
                                    await _videoInfoCommandRepository.UpdateAsync(video);
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

        [HttpGet]
        public async Task<IActionResult> Filter(string? transIdOrPayment, bool? label, bool? noLabel, int page = 1,
            int pageSize = 20)
        {
            QueryOptionsBuilder<VideoInfo> queryOptionBuilder = new();
            queryOptionBuilder.Skip(pageSize * (page - 1));
            queryOptionBuilder.Take(pageSize);
            if (!string.IsNullOrEmpty(transIdOrPayment))
            {
                queryOptionBuilder.Where(v => v.TransID.ToLower().Contains(transIdOrPayment.ToLower()) ||
                                              v.Payment.ToLower().Contains(transIdOrPayment.ToLower()));
            }

            if (label != null)
            {
                if (label.Value)
                {
                    queryOptionBuilder.Where(x => string.IsNullOrEmpty(x.Label) == false);
                }
            }
            
            if (noLabel != null)
            {
                if (noLabel.Value)
                {
                    queryOptionBuilder.Where(x => string.IsNullOrEmpty(x.Label) == true);
                }
            }
            var list = await videoInfoQueryRepository.FilterAsync(queryOptionBuilder.Build());
            return Ok(
                new ApiResponse<List<VideoInfo>>()
                {
                    Data = list,
                    Message = "",
                });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateVideo(UpdateVideo model)
        {
            var video = await videoInfoQueryRepository.GetByIdAsync(model.Id);
            if (video == null)
            {
                return BadRequest(new ApiResponse<object>()
                {
                    Message = "Không tìm thấy video."
                });
            }
            video.Label =  model.Label;
            await videoInfoCommandRepository.UpdateAsync(video);
            
            return Ok(new ApiResponse<object>()
            {
                Message = "Cập nhật thành công."
            });
        }

        [HttpPost]
        public async Task<IActionResult> ExportExcel(ExportExcel exportExcel)
        {
            var data = await videoInfoQueryRepository.GetAllAsync();
            var byteData = videoExcelService.ExportExcel(data.ToList());

            if (exportExcel.ClearAllData)
            {
                 await videoInfoCommandRepository.DeleteRangeAsync(data.ToList());
            }
            return File(byteData, 
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                "Export.xlsx");
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