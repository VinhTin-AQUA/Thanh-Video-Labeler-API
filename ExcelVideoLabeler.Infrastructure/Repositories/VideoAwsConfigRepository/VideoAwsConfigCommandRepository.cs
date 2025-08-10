using ExcelVideoLabeler.Domain.Entities.VideoAws;
using ExcelVideoLabeler.Infrastructure.DataContext;
using ExcelVideoLabeler.Infrastructure.Repositories.Common;

namespace ExcelVideoLabeler.Infrastructure.Repositories.VideoAwsConfigRepository
{
    public interface IVideoAwsConfigCommandRepository : ICommandRepository<VideoAwsConfig>
    {
    }

    public class VideoAwsConfigCommandRepository(AppDbContext context)
        : CommandRepository<VideoAwsConfig>(context), IVideoAwsConfigCommandRepository
    {
    }
}
