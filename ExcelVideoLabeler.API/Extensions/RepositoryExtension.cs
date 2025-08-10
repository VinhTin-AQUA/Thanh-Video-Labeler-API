using ExcelVideoLabeler.Infrastructure.Repositories.ConfigRepository;
using ExcelVideoLabeler.Infrastructure.Repositories.SheetRepository;
using ExcelVideoLabeler.Infrastructure.Repositories.VideoAwsConfigRepository;
using ExcelVideoLabeler.Infrastructure.Repositories.VideoAwsInfoRepository;
using ExcelVideoLabeler.Infrastructure.Repositories.VideoInfoRepository;
using IVideoAwsConfigCommandRepository = ExcelVideoLabeler.Infrastructure.Repositories.VideoAwsConfigRepository.IVideoAwsConfigCommandRepository;
using VideoAwsConfigCommandRepository = ExcelVideoLabeler.Infrastructure.Repositories.VideoAwsConfigRepository.VideoAwsConfigCommandRepository;

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

            services.AddScoped<ISheetCommandRepository, SheetCommandRepository>();
            services.AddScoped<ISheetQueryRepository, SheetQueryRepository>();
            
            services.AddScoped<IVideoAwsConfigCommandRepository, VideoAwsConfigCommandRepository>();
            services.AddScoped<IVideoAwsConfigQueryRepository, VideoAwsConfigQueryRepository>();
            
            services.AddScoped<IVideoAwsInfoCommandRepository, VideoAwsInfoCommandRepository>();
            services.AddScoped<IVideoAwsInfoQueryRepository, VideoAwsInfoQueryRepository>();
        }
    }
}