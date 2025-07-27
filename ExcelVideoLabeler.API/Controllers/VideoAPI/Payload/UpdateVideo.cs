namespace ExcelVideoLabeler.API.Controllers.VideoAPI.Payload
{
    public class UpdateVideo
    {
        public int Id { get; set; }
        public string Label { get; set; } = string.Empty;
    }
}