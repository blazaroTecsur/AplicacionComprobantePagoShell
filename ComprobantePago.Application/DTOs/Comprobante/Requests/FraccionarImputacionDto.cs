namespace ComprobantePago.Application.DTOs.Comprobante.Requests
{
    public class FraccionarImputacionRequest
    {
        public string Folio { get; set; } = string.Empty;
        public List<ImputacionFraccionDto> LineasGravado { get; set; } = new();
        public List<ImputacionFraccionDto> LineasExento  { get; set; } = new();
    }

    public class ImputacionFraccionDto
    {
        public decimal Monto { get; set; }
    }
}
