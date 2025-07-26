using ExcelVideoLablerAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ExcelVideoLablerAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<VideoInfo> VideoInfos { get; set; }
        public DbSet<Config> Config { get; set; }
    }
}