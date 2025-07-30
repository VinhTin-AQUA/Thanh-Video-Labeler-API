namespace ExcelVideoLabeler.API.Controllers.VideoAPI.Payload
{
    public class InitDownloadRequest
    {
        public string SheetCode { get; set; } = string.Empty;
        public string SheetName { get; set; } = string.Empty;
    }
}
