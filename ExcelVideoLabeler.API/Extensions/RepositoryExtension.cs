using ExcelVideoLabler.Infrastructure.Repositories.ConfigRepository;
using ExcelVideoLabler.Infrastructure.Repositories.VideoInfoRepository;

namespace ExcelVideoLabler.API.Extensions
{
    public static class RepositoryExtension
    {
        public static void AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IVideoInfoCommandRepository, VideoInfoCommandRepository>();
            services.AddScoped<IVideoInfoQueryRepository, VideoInfoQueryRepository>();

            services.AddScoped<IConfigCommandRepository, ConfigCommandRepository>();
            services.AddScoped<IConfigQueryRepository, ConfigQueryRepository>();
        }
    }
}