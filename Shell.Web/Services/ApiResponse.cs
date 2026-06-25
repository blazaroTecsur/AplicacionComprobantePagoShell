namespace Shell.Web.Services
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public ApiErrorDetail? Error { get; set; }
        public static ApiResponse<T> Fail(ApiErrorDetail error)
           => new() { Success = false, Error = error };
        public static ApiResponse<T> Ok(T data)
            => new() { Success = true, Data = data };
    }
}