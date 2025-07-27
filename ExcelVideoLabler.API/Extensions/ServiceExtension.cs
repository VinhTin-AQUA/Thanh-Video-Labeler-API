using ExcelVideoLabler.API.Hubs;
using ExcelVideoLabler.API.Services;

namespace ExcelVideoLabler.API.Extensions
{
    public static class ServiceExtension
    {
        public static void AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<ConfigService>();
            services.AddSingleton<VideoService>();
            services.AddSingleton<FileService>();
            services.AddSingleton<VideoExcelService>();
            
           services.AddSingleton<VideoDownloadHubService>();
        }
    }
}