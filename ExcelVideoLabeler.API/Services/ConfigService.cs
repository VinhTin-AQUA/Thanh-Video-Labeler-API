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
                    SheetName = "",
                    SheetCode = "",
                };

                await configCommandRepository.AddAsync(defaultSetting);
                Config = defaultSetting;
            }
            else
            {
                Config = existing;
            }
        }

        //public void Update(Config updatedSetting)
        //{
        //    if (!string.IsNullOrEmpty(updatedSetting.ExceFileName))
        //    {
        //        Config.ExceFileName = updatedSetting.ExceFileName;
        //    }

        //    if (!string.IsNullOrEmpty(updatedSetting.SheetName))
        //    {
        //        Config.SheetName = updatedSetting.SheetName;
        //    }

        //    if (!string.IsNullOrEmpty(updatedSetting.SheetCode))
        //    {
        //        Config.SheetCode = updatedSetting.SheetCode;
        //    }
        //}
    }
}