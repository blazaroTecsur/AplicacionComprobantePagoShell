namespace ComprobantePago.Application.DTOs.Comprobante.Requests
{
    public class FraccionarImputacionRequest
    {
        public string Folio { get; set; } = string.Empty;
        public List<ImputacionFraccionDto> Lineas { get; set; } = new();
    }

    public class ImputacionFraccionDto
    {
        public decimal MontoNeto { get; set; }
        public decimal MontoIgv  { get; set; }
    }
}
