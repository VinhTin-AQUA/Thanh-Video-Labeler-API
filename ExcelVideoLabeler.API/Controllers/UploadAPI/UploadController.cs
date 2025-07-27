using ExcelVideoLabeler.API.Constants;
using ExcelVideoLabeler.API.Controllers.Common;
using ExcelVideoLabeler.API.Controllers.UploadAPI.Payload;
using ExcelVideoLabeler.API.Services;
using ExcelVideoLabeler.Domain.Entities;
using ExcelVideoLabeler.Infrastructure.Repositories.ConfigRepository;
using ExcelVideoLabeler.Infrastructure.Repositories.VideoInfoRepository;
using Microsoft.AspNetCore.Mvc;

namespace ExcelVideoLabeler.API.Controllers.UploadAPI
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly FileService fileService;
        private readonly ConfigService configService;
        private readonly IConfigCommandRepository configCommandRepository;
        private readonly IWebHostEnvironment env;

        public UploadController(FileService fileService, 
            ConfigService configService,
            IConfigCommandRepository  configCommandRepository,
            IWebHostEnvironment env)
        {
            this.fileService = fileService;
            this.configService = configService;
            this.configCommandRepository = configCommandRepository;
            this.env = env;
        }

        [HttpPost]
        public async Task<IActionResult> UploadExcel([FromForm] UploadFile uploadFile)
        {
            if (uploadFile.File == null || uploadFile.File.Length == 0)
            {
                return BadRequest(new ApiResponse<object>()
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
                Id = 1,
                ExceFileName = uploadFile.File.FileName,
                SheetIndex = 0,
                TotalDownloaded = 0
            };
            // update filePath
            configService.Update(newVideo);
            await configCommandRepository.UpdateAsync(ConfigService.Config);
            return Ok(new ApiResponse<object>
            {
                Data = null,
                Message = "Upload thành công."
            });
        }
    }
}