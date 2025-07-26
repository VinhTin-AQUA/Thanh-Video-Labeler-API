using ExcelVideoLablerAPI.Common.Constants;
using ExcelVideoLablerAPI.Common.Responses;
using ExcelVideoLablerAPI.Models;
using ExcelVideoLablerAPI.Repositories;
using ExcelVideoLablerAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExcelVideoLablerAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly FileService fileService;
        private readonly IVideoInfoRepository videoInfoRepository;
        private readonly ConfigService configService;
        private readonly IConfigRepository configRepository;
        private readonly IWebHostEnvironment env;

        public UploadController(FileService fileService, 
            IVideoInfoRepository  videoInfoRepository,
            ConfigService configService,
            IConfigRepository  configRepository,
            IWebHostEnvironment env)
        {
            this.fileService = fileService;
            this.videoInfoRepository = videoInfoRepository;
            this.configService = configService;
            this.configRepository = configRepository;
            this.env = env;
        }

        [HttpPost]
        public async Task<IActionResult> UploadExcel(IFormFile file)
        {
            if (file.Length == 0)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Data = null,
                    Message = "File không hợp lệ."
                });
            }

            _ = await fileService.SaveFile(file, Path.Combine(env.WebRootPath, FolderConstants.ExcelFolder));

            var newVideo = new Config()
            {
                ExceFileName = file.FileName,
                SheetIndex = 0,
                TotalDownloaded = 0
            };
            await configRepository.Update(newVideo);
            
            // update filePath
            configService.Update(newVideo);
            
            return Ok(new ApiResponse<object>
            {
                Data = null,
                Message = "Upload thành công."
            });
        }
    }
}