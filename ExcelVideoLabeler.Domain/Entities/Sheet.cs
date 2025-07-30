using ExcelVideoLabeler.Domain.Entities.Common;
using ExcelVideoLabeler.Domain.Enums;

namespace ExcelVideoLabeler.Domain.Entities
{
    public class Sheet : Entity
    {
        public string SheetName { get; set; } = string.Empty;
        public string SheetCode { get; set; } = string.Empty;
        public ESheetStatus SheetStatus { get; set; } = ESheetStatus.Pending;
    }
}
