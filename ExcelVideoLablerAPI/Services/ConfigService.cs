using ExcelVideoLablerAPI.Data;
using ExcelVideoLablerAPI.Models;
using ExcelVideoLablerAPI.Repositories;

namespace ExcelVideoLablerAPI.Services
{
    public class ConfigService
    {
        private readonly object _lock = new();
        public static Config Config = new();

        public async Task Init(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var configRepository = scope.ServiceProvider.GetRequiredService<IConfigRepository>();
            var existing = await configRepository.GetById(1);
            if (existing == null)
            {
                var defaultSetting = new Config
                {
                    Id = 1,
                    ExceFileName = "",
                    TotalDownloaded = 0,
                    TotalToDownload = 0,
                    SheetIndex = 0,
                    TotalSheet = 0,
                    RowIndex = 1,
                };

                await configRepository.Add(defaultSetting);
                Config = defaultSetting;
            }
            else
            {
                Config = existing;
            }
        }

        public void Update(Config updatedSetting)
        {
            if (!string.IsNullOrEmpty(updatedSetting.ExceFileName))
            {
                Config.ExceFileName = updatedSetting.ExceFileName;
            }

            if (updatedSetting.TotalDownloaded != -1)
            {
                Config.TotalDownloaded = updatedSetting.TotalDownloaded;
            }
        
            if (updatedSetting.TotalToDownload != -1)
            {
                Config.TotalToDownload = updatedSetting.TotalToDownload;
            }
        
            if (updatedSetting.SheetIndex != -1)
            {
                Config.SheetIndex = updatedSetting.SheetIndex;
            }
            
            if (updatedSetting.TotalSheet != -1)
            {
                Config.TotalSheet = updatedSetting.TotalSheet;
            }
            
            if (updatedSetting.RowIndex != -1)
            {
                Config.RowIndex = updatedSetting.RowIndex;
            }
        }
    }
}