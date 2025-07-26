using ExcelVideoLabler.Domain.Entities;
using ExcelVideoLabler.Infrastructure.DataContext;
using ExcelVideoLabler.Infrastructure.Repositories.Common;

namespace ExcelVideoLabler.Infrastructure.Repositories.VideoInfoRepository
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