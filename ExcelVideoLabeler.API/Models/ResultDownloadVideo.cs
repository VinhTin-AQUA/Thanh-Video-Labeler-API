namespace ExcelVideoLabeler.API.Models
{
    public class ResultDownloadVideo
    {
        public bool IsSuccess { get; set; }
        public string TransId { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public int TotalDownloadsSuccess { get; set; } 
        public int TotalDownloadsFailed  { get; set; }
    }
}