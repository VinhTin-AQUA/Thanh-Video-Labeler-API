using ExcelVideoLabeler.Domain.Entities.Common;

namespace ExcelVideoLabeler.Domain.Entities.VideoAws
{
    public class VideoAwsConfig : Entity
    {
        public string FileName { get; set; } = string.Empty;
        public int TotalVideoAws { get; set; } = 0;
        public int TotalVideoAwsDownloaded { get; set; } = 0;
    }
}