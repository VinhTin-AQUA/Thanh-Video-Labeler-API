using ExcelVideoLabeler.Domain.Entities.VideoAws;
using ExcelVideoLabeler.Infrastructure.DataContext;
using ExcelVideoLabeler.Infrastructure.Repositories.Common;

namespace ExcelVideoLabeler.Infrastructure.Repositories.VideoAwsInfoRepository
{
    public interface IVideoAwsInfoCommandRepository : ICommandRepository<VideoAwsInfo>
    {
    }

    public class VideoAwsInfoCommandRepository(AppDbContext context)
        : CommandRepository<VideoAwsInfo>(context), IVideoAwsInfoCommandRepository
    {
    }
}
