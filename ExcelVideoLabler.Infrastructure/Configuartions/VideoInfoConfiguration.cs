using ExcelVideoLabler.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExcelVideoLabler.Infrastructure.Configuartions
{
    public class VideoInfoConfiguration: IEntityTypeConfiguration<VideoInfo>
    {
        public void Configure(EntityTypeBuilder<VideoInfo> builder)
        {
            // builder
            //     .HasIndex(s => s.SchoolYearId)
            //     .HasDatabaseName("IX_Class_SchoolYearId");
        }
    }
}