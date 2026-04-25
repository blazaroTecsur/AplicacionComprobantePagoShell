namespace Shell.Web.Services
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public ApiErrorDetail? Error { get; set; }
    }
}