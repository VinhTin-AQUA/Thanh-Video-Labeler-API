using System.ComponentModel.DataAnnotations.Schema;

namespace ExcelVideoLablerAPI.Models
{
    [Table("Config")]
    public class Config
    {
        public int Id { get; set; }
        public int TotalDownloaded { get; set; } = -1; // số lượng đã 
        public string? ExceFileName { get; set; } = null; // tên file
        public int TotalToDownload { get; set; } = -1; // số lượng download mỗi lượt
        public int SheetIndex { get; set; } = -1; //
        public int TotalSheet { get; set; } = -1;
        public int RowIndex { get; set; } = -1;
    }
}