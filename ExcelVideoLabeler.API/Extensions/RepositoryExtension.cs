using ExcelVideoLabeler.Infrastructure.Repositories.ConfigRepository;
using ExcelVideoLabeler.Infrastructure.Repositories.VideoInfoRepository;

namespace ExcelVideoLabeler.API.Extensions
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