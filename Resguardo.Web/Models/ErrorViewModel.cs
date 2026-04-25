namespace Resguardo.Web.Models
{
    public class ErrorViewModel
    {
        public int StatusCode { get; set; }
        public string? TraceId { get; set; }
        public string UserMessage { get; set; } = string.Empty;
    }
}
