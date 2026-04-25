namespace Resguardo.Application.DTOs.Response
{
    public class SeguridadErrorResponse
    {
        public string Code { get; set; } = null!;
        public string UserMessage { get; set; } = null!;
        public string? TechnicalMessage { get; set; }
        public Dictionary<string, string[]>? ValidationErrors { get; set; }
        public string? TraceId { get; set; }
    }
}