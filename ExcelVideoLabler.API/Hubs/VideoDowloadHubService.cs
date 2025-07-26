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

        public async Task RecieveTotalVideo(int totalDownloadsSuccess, int totalDownloadsFailed)
        {
           await videoDowloadHub.Clients.Client(VideoDowloadHub.ConnectionId)
                .RecieveTotalVideo(totalDownloadsSuccess, totalDownloadsFailed);
        }
    }
}