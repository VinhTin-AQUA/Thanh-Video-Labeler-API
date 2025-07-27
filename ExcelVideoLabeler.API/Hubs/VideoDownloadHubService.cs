using ExcelVideoLabeler.API.Models;
using Microsoft.AspNetCore.SignalR;

namespace ExcelVideoLabeler.API.Hubs
{
    public class VideoDownloadHubService
    {
        private readonly IHubContext<VideoDowloadHub, IVideoDowloadHub> videoDowloadHub;

        public VideoDownloadHubService(IHubContext<VideoDowloadHub, IVideoDowloadHub>  videoDowloadHub)
        {
            this.videoDowloadHub = videoDowloadHub;
        }

        public async Task RecieveTotalVideo(ResultDownloadVideo resultDownloadVideo)
        {
           await videoDowloadHub.Clients.Client(VideoDowloadHub.ConnectionId)
                .RecieveResultDownloadVideo(resultDownloadVideo);
        }
        
        public async Task SendDownloadFinish(bool isFinish)
        {
            await videoDowloadHub.Clients.Client(VideoDowloadHub.ConnectionId)
                .SendDownloadFinish(isFinish);
        }
    }
}