using ExcelVideoLabler.API.Models;
using Microsoft.AspNetCore.SignalR;

namespace ExcelVideoLabler.API.Hubs
{
    public class VideoDowloadHubService
    {
        private readonly IHubContext<VideoDowloadHub, IVideoDowloadHub> videoDowloadHub;

        public VideoDowloadHubService(IHubContext<VideoDowloadHub, IVideoDowloadHub>  videoDowloadHub)
        {
            this.videoDowloadHub = videoDowloadHub;
        }

        public async Task RecieveTotalVideo(ResultDownloadVideo resultDownloadVideo)
        {
           await videoDowloadHub.Clients.Client(VideoDowloadHub.ConnectionId)
                .RecieveTotalVideo(resultDownloadVideo);
        }
    }
}