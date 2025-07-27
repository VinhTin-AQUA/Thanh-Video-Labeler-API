using ExcelVideoLabeler.Domain.Entities;
using ExcelVideoLabeler.Infrastructure.DataContext;
using ExcelVideoLabeler.Infrastructure.Repositories.Common;

namespace ExcelVideoLabeler.Infrastructure.Repositories.VideoInfoRepository
{
    public interface IVideoInfoCommandRepository : ICommandRepository<VideoInfo>
    {
        Task AddWithOther();
    }

    public class VideoInfoCommandRepository(AppDbContext context)
        : CommandRepository<VideoInfo>(context), IVideoInfoCommandRepository
    {
        public Task AddWithOther()
        {
            throw new NotImplementedException();
        }
    }
}