using ExcelVideoLablerAPI.Data;
using ExcelVideoLablerAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ExcelVideoLablerAPI.Repositories
{
    public interface IConfigRepository
    {
        Task Add(Config updatedSetting);
        Task Update(Config updatedSetting);
        Task<Config?> GetById(int Id);
        Task<bool> SaveChange();
    }
    
    public class ConfigRepository : IConfigRepository
    {
        private readonly AppDbContext context;
        
        public ConfigRepository(AppDbContext context)
        {
            this.context = context;
        }

        public async Task Add(Config updatedSetting)
        {
            await context.Config.AddAsync(updatedSetting);
            await SaveChange();
        }

        public async Task Update(Config updatedSetting)
        {
            var settingInDb = await GetById(1);
            if (settingInDb == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(updatedSetting.ExceFileName))
            {
                settingInDb.ExceFileName = updatedSetting.ExceFileName;
            }

            if (updatedSetting.TotalDownloaded != -1)
            {
                settingInDb.TotalDownloaded = updatedSetting.TotalDownloaded;
            }
            
            if (updatedSetting.TotalToDownload != -1)
            {
                settingInDb.TotalToDownload = updatedSetting.TotalToDownload;
            }
            
            if (updatedSetting.SheetIndex != -1)
            {
                settingInDb.SheetIndex = updatedSetting.SheetIndex;
            }
            
            if (updatedSetting.TotalSheet != -1)
            {
                settingInDb.TotalSheet = updatedSetting.TotalSheet;
            }
            
            if (updatedSetting.RowIndex != -1)
            {
                settingInDb.RowIndex = updatedSetting.RowIndex;
            }
            
            context.Config.Update(settingInDb);
            await SaveChange();
        }

        public async Task<Config?> GetById(int id)
        {
            var config = await context.Config
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();
            return config;
        }

        public async Task<bool> SaveChange()
        {
            var r = await context.SaveChangesAsync();
            return r > 0;
        }
    }
}