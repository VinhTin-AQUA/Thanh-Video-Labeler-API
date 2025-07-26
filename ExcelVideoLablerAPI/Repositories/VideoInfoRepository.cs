using ExcelVideoLablerAPI.Data;
using ExcelVideoLablerAPI.Enums;
using ExcelVideoLablerAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ExcelVideoLablerAPI.Repositories
{
    public interface IVideoInfoRepository
    {
        Task<bool> SaveChange();
        Task<VideoInfo> Add(VideoInfo videoInfo);
        Task<List<VideoInfo>> AddRange(List<VideoInfo> videoInfos);
        Task<VideoInfo?> GetById(string transID);
        Task<List<VideoInfo>> GetAll(VideoStatus videoStatus);
        Task<List<VideoInfo>> GetAll();
        Task<VideoInfo> Update(VideoInfo videoInfo);
        Task<bool> Delete(string transID);
        Task UpdateRange(List<VideoInfo> videoInfos);
    }
    
    public class VideoInfoRepository : IVideoInfoRepository
    {
        private readonly AppDbContext context;

        public VideoInfoRepository(AppDbContext context)
        {
            this.context = context;
        }

        public async Task<bool> SaveChange()
        {
            var r = await context.SaveChangesAsync();
            return r > 0;
        }
        
        public async Task<VideoInfo> Add(VideoInfo videoInfo)
        {
            await context.VideoInfos.AddAsync(videoInfo);
            await SaveChange();
            return videoInfo;
        }

        public async Task<List<VideoInfo>> AddRange(List<VideoInfo> videoInfos)
        {
            await context.VideoInfos.AddRangeAsync(videoInfos);
            await SaveChange();
            return videoInfos;
        }

        public async Task<VideoInfo?> GetById(string transID)
        {
            var videoInfo = await context.VideoInfos
                .Where(x => x.TransID == transID)
                .FirstOrDefaultAsync();
            return videoInfo;
        }

        public async Task<List<VideoInfo>> GetAll(VideoStatus videoStatus)
        {
            var r = await context
                .VideoInfos
                .Where(x => x.VideoStatus == videoStatus)
                .ToListAsync();
            return r;
        }
        
        public async Task<List<VideoInfo>> GetAll()
        {
            var r = await context
                .VideoInfos
                .ToListAsync();
            return r;
        }

        public async Task<VideoInfo> Update(VideoInfo videoInfo)
        {
            context.VideoInfos.Update(videoInfo);
            await SaveChange();
            return videoInfo;
        }

        public async Task<bool> Delete(string transID)
        {
            var videoInfo = await GetById(transID);
            if (videoInfo == null)
            {
                return false;
            }
            context.VideoInfos.Remove(videoInfo);
            var r = await SaveChange();
            return r;
        }

        public async Task UpdateRange(List<VideoInfo> videoInfos)
        {
            context.VideoInfos.UpdateRange(videoInfos);
            await SaveChange();
        }
    }
}