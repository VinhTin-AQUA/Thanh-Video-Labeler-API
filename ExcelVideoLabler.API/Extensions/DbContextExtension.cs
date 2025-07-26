using ExcelVideoLabler.Infrastructure.DataContext;
using Microsoft.EntityFrameworkCore;

namespace ExcelVideoLabler.API.Extensions
{
    public static class DbContextExtension
    {
        public static IServiceCollection AddSqliteDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));
            
            // services.AddDbContextPool<AppDbContext>(options =>
            //     options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));
            
            return services;
        }
    }
}