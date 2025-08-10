using ExcelVideoLabeler.Domain.Entities.VideoAws;
using ExcelVideoLabeler.Infrastructure.DataContext;
using ExcelVideoLabeler.Infrastructure.Repositories.Common;

namespace ExcelVideoLabeler.Infrastructure.Repositories.VideoAwsConfigRepository
{
    public interface IVideoAwsConfigQueryRepository : IQueryRepository<VideoAwsConfig>
    {
    }

    public class VideoAwsConfigQueryRepository(AppDbContext context)
        : QueryRepository<VideoAwsConfig>(context), IVideoAwsConfigQueryRepository
    {
    }
}
