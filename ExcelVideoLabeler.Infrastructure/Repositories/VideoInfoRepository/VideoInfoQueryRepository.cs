using ExcelVideoLabeler.Domain.Entities;
using ExcelVideoLabeler.Domain.Enums;
using ExcelVideoLabeler.Infrastructure.DataContext;
using ExcelVideoLabeler.Infrastructure.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace ExcelVideoLabeler.Infrastructure.Repositories.VideoInfoRepository
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