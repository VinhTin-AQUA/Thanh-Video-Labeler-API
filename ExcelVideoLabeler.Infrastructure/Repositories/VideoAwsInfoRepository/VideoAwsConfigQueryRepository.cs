using ExcelVideoLabeler.Domain.Entities.VideoAws;
using ExcelVideoLabeler.Infrastructure.DataContext;
using ExcelVideoLabeler.Infrastructure.Repositories.Common;

namespace ExcelVideoLabeler.Infrastructure.Repositories.VideoAwsInfoRepository
{
    public interface IVideoAwsInfoQueryRepository : IQueryRepository<VideoAwsInfo>
    {
    }

    public class VideoAwsInfoQueryRepository(AppDbContext context)
        : QueryRepository<VideoAwsInfo>(context), IVideoAwsInfoQueryRepository
    {
    }
}
