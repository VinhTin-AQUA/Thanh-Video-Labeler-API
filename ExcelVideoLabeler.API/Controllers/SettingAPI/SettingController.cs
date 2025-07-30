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
        private readonly IConfigCommandRepository configCommandRepository;
        private readonly IVideoInfoCommandRepository videoInfoCommandRepository;
        private readonly IVideoInfoQueryRepository videoInfoQueryRepository;
        private readonly IWebHostEnvironment env;

        public SettingController(IConfigCommandRepository configCommandRepository,
            IVideoInfoCommandRepository videoInfoCommandRepository,
            IVideoInfoQueryRepository videoInfoQueryRepository,
            IWebHostEnvironment env)
        {
            this.configCommandRepository = configCommandRepository;
            this.videoInfoCommandRepository = videoInfoCommandRepository;
            this.videoInfoQueryRepository = videoInfoQueryRepository;
            this.env = env;
        }

        [HttpGet]
        public IActionResult GetSettingInfo()
        {
            return Ok(new ApiResponse<object>()
            {
                Data = new
                {
                    FileName = string.IsNullOrEmpty(ConfigService.Config.ExceFileName) ? "Empty" : ConfigService.Config.ExceFileName,
                    SheetName = string.IsNullOrEmpty(ConfigService.Config.SheetName) ? "Empty" : ConfigService.Config.SheetName,
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
                ConfigService.Config.SheetName = "";
                ConfigService.Config.SheetCode = "";
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