using ExcelVideoLabeler.Domain.Entities;
using ExcelVideoLabeler.Infrastructure.Repositories.ConfigRepository;

namespace ExcelVideoLabeler.API.Services
{
    public class ConfigService
    {
        private readonly object _lock = new();
        public static Config Config = new();

        public async Task Init(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var configQueryRepository = scope.ServiceProvider.GetRequiredService<IConfigQueryRepository>();
            var configCommandRepository = scope.ServiceProvider.GetRequiredService<IConfigCommandRepository>();
            var existing = await configQueryRepository.GetByIdAsync(1);
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

                await configCommandRepository.AddAsync(defaultSetting);
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