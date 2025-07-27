using ExcelVideoLabeler.API.Hubs;
using ExcelVideoLabeler.API.Services;

namespace ExcelVideoLabeler.API.Extensions
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