using Aspose.Cells;
using ExcelVideoLabeler.API.Constants;
using ExcelVideoLabeler.Domain.Entities;
using ExcelVideoLabeler.Domain.Enums;

namespace ExcelVideoLabeler.API.Services
{
    public class VideoExcelService
    {
        public void ReadExcel(IFormFile file)
        {
            using var stream = new MemoryStream();
            file.CopyTo(stream);
            stream.Position = 0;

            var workbook = new Workbook(stream);
            var worksheet = workbook.Worksheets[0];
            var cells = worksheet.Cells;

            int rowCount = cells.MaxDataRow;
            int colCount = cells.MaxDataColumn;

            // Tạo dictionary map header của file Excel → index
            var excelHeaderMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int col = 0; col <= colCount; col++)
            {
                string header = cells[0, col].StringValue.Trim();
                if (!string.IsNullOrWhiteSpace(header))
                    excelHeaderMap[header] = col;
            }

            var result = new List<Dictionary<string, string>>();
            for (int row = 1; row <= rowCount; row++)
            {
                var rowData = new Dictionary<string, string>();
                foreach (var header in VideoConstants.ExpectedHeaders)
                {
                    if (excelHeaderMap.ContainsKey(header))
                    {
                        int colIndex = excelHeaderMap[header];
                        rowData[header] = cells[row, colIndex].StringValue?.Trim() ?? "";
                    }
                    else
                    {
                        // Header không tồn tại trong Excel, gán giá trị rỗng
                        rowData[header] = "";
                    }
                }

                result.Add(rowData);
            }

            _ = MapToVideoInfos(result);
        }

        public List<VideoInfo> ReadDataFromSheet(Worksheet worksheet)
        {
            var cells = worksheet.Cells;
            int rowCount = cells.MaxDataRow;
            int colCount = cells.MaxDataColumn;

            // Tạo dictionary map header của file Excel → index
            var excelHeaderMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int col = 0; col <= colCount; col++)
            {
                string header = cells[0, col].StringValue.Trim();
                if (!string.IsNullOrWhiteSpace(header))
                    excelHeaderMap[header] = col;
            }

            int remaining = VideoConstants.TotalVideoToDownload;

            var result = new List<Dictionary<string, string>>();
            for (int row = ConfigService.Config.RowIndex; row <= rowCount && remaining > 0; row++)
            {
                var rowData = new Dictionary<string, string>();
                foreach (var header in VideoConstants.ExpectedHeaders)
                {
                    if (excelHeaderMap.ContainsKey(header))
                    {
                        int colIndex = excelHeaderMap[header];
                        rowData[header] = cells[row, colIndex].StringValue?.Trim() ?? "";
                    }
                    else
                    {
                        // Header không tồn tại trong Excel, gán giá trị rỗng
                        rowData[header] = "";
                    }
                }
                remaining--;
                result.Add(rowData);
            }

            var r = MapToVideoInfos(result);
            return r;
        }
        
        private List<VideoInfo> MapToVideoInfos(List<Dictionary<string, string>> rowData)
        {
            List<VideoInfo> r = [];
            foreach (var row in rowData)
            {
                VideoInfo info = new()
                {
                    TransID = row["TransID"],
                    TransNB = row["TransNB"],
                    TotalAmount = row["Total_amount"],
                    DVRStart = row["DVRStart"],
                    Channel = row["Channel"],
                    StartShift = row["start_shift"],
                    EndShift = row["end_shift"],
                    EmployeeID = row["EmployeeID"],
                    RegisterName = row["Register_Name"],
                    LoyaltyCard = row["Loyalty_card"],
                    SiteName = row["SiteName"],
                    SiteEmployee = row["SiteEmployee"],
                    ExceptionAmount = row["exception_amount"],
                    TOperatorID = row["T_OperatorID"],
                    TPACID = row["T_PACID"],
                    City = row["City"],
                    RegionName = row["RegionName"],
                    Division = row["Division"],
                    ExceptionType = row["exception_type"],
                    Payment = row["Payment"],
                    CardNo = row["CardNo"],
                    Desc = row["Desc"],
                    Link = row["link"],
                    Label = row["Label"],
                    TransDate = row["TransDate"],
                    Assign = row["Assign"],
                    HighRisk = row["high_risk"],
                    PreTransID = row["pre_TransID"],
                    PreTransDate = row["Pre_TransDate"],
                    PreTotalAmount = row["Pre_Total_amount"],
                    HasRedeemLater = row["has_redeem_later"],
                    YPredProba = row["y_pred_proba"],
                    Bucket = row["bucket"],
                    BucketSmall = row["bucket_small"],
                    TransDateWeekdayMask = row["TransDate_weekday_mask"],
                    TransDateFormatted = row["TransDate_formatted"],
                    VideoStatus = VideoStatus.Pending
                };
                r.Add(info);
            }
            return r;
        }
    }
}