using Aspose.Cells;
using ExcelVideoLabeler.API.Constants;
using ExcelVideoLabeler.API.Controllers.Common;
using ExcelVideoLabeler.API.Services;
using ExcelVideoLabeler.Infrastructure.Repositories.ConfigRepository;
using ExcelVideoLabeler.Infrastructure.Repositories.VideoInfoRepository;
using Microsoft.AspNetCore.Mvc;

namespace ExcelVideoLabeler.API.Controllers.SettingAPI
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class SettingController : ControllerBase
    {
        private readonly IConfigQueryRepository configQueryRepository;
        private readonly IConfigCommandRepository configCommandRepository;
        private readonly IVideoInfoCommandRepository videoInfoCommandRepository;
        private readonly IVideoInfoQueryRepository videoInfoQueryRepository;
        private readonly IWebHostEnvironment env;

        public SettingController(IConfigQueryRepository configQueryRepository,
            IConfigCommandRepository configCommandRepository,
            IVideoInfoCommandRepository videoInfoCommandRepository,
            IVideoInfoQueryRepository videoInfoQueryRepository,
            IWebHostEnvironment env)
        {
            this.configQueryRepository = configQueryRepository;
            this.configCommandRepository = configCommandRepository;
            this.videoInfoCommandRepository = videoInfoCommandRepository;
            this.videoInfoQueryRepository = videoInfoQueryRepository;
            this.env = env;
        }

        [HttpGet]
        public async Task<IActionResult> GetSettingInfo()
        {
            var config = await configQueryRepository.GetByIdAsync(1);
            if (config == null)
            {
                return BadRequest(new ApiResponse<object>()
                {
                    Message = "Error when get Setting Info"
                });
            }

            if (!string.IsNullOrEmpty(ConfigService.Config.ExceFileName))
            {
                string filePath = Path.Combine(env.WebRootPath, FolderConstants.ExcelFolder,ConfigService.Config.ExceFileName);
               
                string sheetName = "";
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var workbook = new Workbook(stream))
                {
                    sheetName = workbook.Worksheets[ConfigService.Config.SheetIndex].Name;
                }
                
                return Ok(new ApiResponse<object>()
                {
                    Data = new
                    {
                        FileName = config.ExceFileName,
                        SheetName = sheetName
                    },
                    Message = ""
                });
            }
            
            return Ok(new ApiResponse<object>()
            {
                Data = new
                {
                    FileName = "Nothing.",
                    SheetName = "Nothing."
                },
                Message = ""
            });
        }

        [HttpPost]
        public async Task<IActionResult> ResetData()
        {
            try
            {
                string filePath = Path.Combine(env.WebRootPath, FolderConstants.ExcelFolder,
                    ConfigService.Config.ExceFileName ?? "");

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
                
                ConfigService.Config.ExceFileName = "";
                ConfigService.Config.ExceFileName = "";
                ConfigService.Config.TotalDownloaded = 0;
                ConfigService.Config.TotalToDownload = 0;
                ConfigService.Config.SheetIndex = 0;
                ConfigService.Config.TotalSheet = 0;
                ConfigService.Config.RowIndex = 1;
                await configCommandRepository.UpdateAsync(ConfigService.Config);
            
                string folderPath = Path.Combine(env.WebRootPath, FolderConstants.VideoFolder);
                if (Directory.Exists(folderPath))
                { string[] files = Directory.GetFiles(folderPath);

                    foreach (var file in files)
                    {
                        System.IO.File.Delete(file);
                    }
                }
                
                var listVideos = await videoInfoQueryRepository.GetAllAsync();
                await videoInfoCommandRepository.DeleteRangeAsync(listVideos.ToList());
            
                return Ok(
                    new ApiResponse<object>()
                    {
                        Message = "Delete Successfully."
                    });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
    }
}