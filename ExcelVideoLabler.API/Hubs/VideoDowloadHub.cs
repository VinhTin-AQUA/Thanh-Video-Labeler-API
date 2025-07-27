using ExcelVideoLabler.API.Models;
using Microsoft.AspNetCore.SignalR;

namespace ExcelVideoLabler.API.Hubs
{
    public interface IVideoDowloadHub
    {
        Task RecieveTotalVideo(ResultDownloadVideo  result);
    }
    
    public class VideoDowloadHub : Hub<IVideoDowloadHub>
    {
        public static string ConnectionId = "";
        
        public override Task OnConnectedAsync()
        {
            if (string.IsNullOrEmpty(ConnectionId))
            {
                ConnectionId  = Context.ConnectionId;
            }
            
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            ConnectionId = "";
            return base.OnDisconnectedAsync(exception);
        }
    }
}