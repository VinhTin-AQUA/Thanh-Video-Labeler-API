namespace ExcelVideoLabeler.API.Controllers.Common
{
    public class ApiResponse<T>
    {
        public T? Data { get; set; }
        public string Message { get; set; } = "";
    }
}