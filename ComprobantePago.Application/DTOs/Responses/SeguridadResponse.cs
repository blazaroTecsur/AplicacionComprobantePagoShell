namespace ComprobantePago.Application.DTOs.Response
{
    public class SeguridadResponse<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public SeguridadErrorResponse? Error { get; set; }
    }
}