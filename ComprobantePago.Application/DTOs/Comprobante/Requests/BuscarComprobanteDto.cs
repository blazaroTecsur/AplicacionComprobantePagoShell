namespace ComprobantePago.Application.DTOs.Comprobante.Requests
{
    public class BuscarComprobanteDto
    {
        public string? Tipo { get; set; }
        public string? Estado { get; set; }
        public string? Proveedor { get; set; }
        public string? Folio { get; set; }
    }
}
