using ExcelVideoLabeler.API.Hubs;
using ExcelVideoLabeler.API.Hubs.VideoAws;
using ExcelVideoLabeler.API.Models;
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
            services.AddSingleton<VideoAwsService>();
            
            services.AddSingleton<VideoDownloadHubService>();
            services.AddSingleton<VideoAwsHubService>();

            services.AddSingleton<AwsConfig>(configuration.GetSection("AwsConfig").Get<AwsConfig>()!);
        }
    }
}