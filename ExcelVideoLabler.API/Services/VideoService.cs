using System.Reflection;
using ExcelVideoLabler.API.Constants;
using ExcelVideoLabler.Domain.Entities;

namespace ExcelVideoLabler.API.Services
{
    public class VideoService
    {
        private readonly IWebHostEnvironment environment;

        public VideoService(IWebHostEnvironment environment)
        {
            this.environment = environment;
        }
        
        public async Task<bool> DownloadVideo(string url, string transId, CancellationToken cancellationToken)
        {
            if (!url.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            
            string folderPath = Path.Combine(environment.WebRootPath, FolderConstants.VideoFolder);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string savePath = Path.Combine(folderPath, $"{transId}.mp4");

            try
            {
                using HttpClient client = new HttpClient();
                using HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                response.EnsureSuccessStatusCode();

                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                await using var fs = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None);

                byte[] buffer = new byte[81920];
                int bytesRead;
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                {
                    await fs.WriteAsync(buffer, 0, bytesRead);
                }

                return true;
            }
            catch (OperationCanceledException)
            {
                if (!File.Exists(savePath))
                    return false;
                File.Delete(savePath);
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task SaveFileVideoInfo(VideoInfo  videoInfo)
        {
            string folderPath = Path.Combine(environment.WebRootPath, FolderConstants.VideoFolder);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            using StreamWriter writer = new StreamWriter(Path.Combine(folderPath, videoInfo.TransID + ".txt"));
            foreach (PropertyInfo prop in typeof(VideoInfo).GetProperties())
            {
                object? value = prop.GetValue(videoInfo, null);
                if (value == null)
                {
                    return;
                }
                writer.WriteLine($"{prop.Name}: {value}");
            }
        }
    }
}