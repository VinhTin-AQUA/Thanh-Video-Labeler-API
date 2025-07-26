using ExcelVideoLabler.Domain.Entities;
using ExcelVideoLabler.Domain.Enums;
using ExcelVideoLabler.Infrastructure.DataContext;
using ExcelVideoLabler.Infrastructure.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace ExcelVideoLabler.Infrastructure.Repositories.VideoInfoRepository
{
    public interface IVideoInfoQueryRepository: IQueryRepository<VideoInfo>
    {
        Task<List<VideoInfo>> GetByStatus(VideoStatus status);
    }
    
    public class VideoInfoQueryRepository(AppDbContext context)
        : QueryRepository<VideoInfo>(context), IVideoInfoQueryRepository
    {
        public Task<List<VideoInfo>> GetByStatus(VideoStatus status)
        {
            throw new NotImplementedException();
        }
    }
}