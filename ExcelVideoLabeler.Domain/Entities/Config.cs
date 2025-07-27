using ExcelVideoLabeler.Domain.Entities.Common;

namespace ExcelVideoLabeler.Domain.Entities
{
    public class Config : Entity
    {
        public int TotalDownloaded { get; set; } = -1; // số lượng đã 
        public string? ExceFileName { get; set; } = null; // tên file
        public int TotalToDownload { get; set; } = -1; // số lượng download mỗi lượt
        public int SheetIndex { get; set; } = -1; //
        public int TotalSheet { get; set; } = -1;
        public int RowIndex { get; set; } = -1;
    }
}