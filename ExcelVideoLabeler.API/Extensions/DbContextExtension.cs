using ExcelVideoLabeler.Infrastructure.DataContext;
using Microsoft.EntityFrameworkCore;

namespace ExcelVideoLabeler.API.Extensions
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

        public static void StartMigrationPending(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate(); 
        }
    }
}