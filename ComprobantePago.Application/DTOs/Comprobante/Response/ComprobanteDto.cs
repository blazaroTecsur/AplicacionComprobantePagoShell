namespace ComprobantePago.Application.DTOs.Comprobante.Response
{
    public sealed class ComprobanteDto
    {
        public string Folio          { get; init; } = string.Empty;
        public string TipoComprobante{ get; init; } = string.Empty;
        public string Serie          { get; init; } = string.Empty;
        public string Numero         { get; init; } = string.Empty;
        public string Proveedor      { get; init; } = string.Empty;
        public string Fecha          { get; init; } = string.Empty;
        public string Moneda         { get; init; } = string.Empty;
        public decimal MontoTotal    { get; init; }
        public string Estado         { get; init; } = string.Empty;
        public int? VoucherSyteline  { get; init; }
    }
}
