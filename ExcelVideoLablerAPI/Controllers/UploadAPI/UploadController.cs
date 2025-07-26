using ExcelVideoLablerAPI.Common.Constants;
using ExcelVideoLablerAPI.Common.Responses;
using ExcelVideoLablerAPI.Controllers.UploadAPI.Payload;
using ExcelVideoLablerAPI.Models;
using ExcelVideoLablerAPI.Repositories;
using ExcelVideoLablerAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExcelVideoLablerAPI.Controllers.UploadAPI
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
        public async Task<IActionResult> UploadExcel([FromForm] UploadFile uploadFile)
        {
            if (uploadFile.File == null || uploadFile.File.Length == 0)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Data = null,
                    Message = "File không hợp lệ."
                });
            }

            string filePath = Path.Combine(env.WebRootPath, FolderConstants.ExcelFolder);
            if (!uploadFile.IsAccepted && System.IO.File.Exists(Path.Combine(filePath, uploadFile.File.FileName)))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Data = new
                    {
                        IsAccepted = !uploadFile.IsAccepted,
                    },
                    Message = "File hiện đã tồn tại trong hệ thống. Nếu tiếp tục upload dữ liệu cũ sẽ mất."
                });
            }
            
            _ = await fileService.SaveFile(uploadFile.File, filePath);
            var newVideo = new Config()
            {
                ExceFileName = uploadFile.File.FileName,
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