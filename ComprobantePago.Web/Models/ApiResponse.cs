namespace ComprobantePago.Web.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public ErrorDetail? Error { get; set; }

        public static ApiResponse<T> Ok(T data) =>
            new() { Success = true, Data = data };

        public static ApiResponse<T> Fail(ErrorDetail error) =>
            new() { Success = false, Error = error };
    }

    public class ErrorDetail
    {
        public string Code { get; set; } = string.Empty;
        public string UserMessage { get; set; } = string.Empty;
        public string? TechnicalMessage { get; set; }
        public Dictionary<string, string[]>? ValidationErrors { get; set; }
        public string? TraceId { get; set; }
    }
}
