namespace ComprobantePago.Application.DTOs.Comprobante.Response
{
    public sealed class ImputacionDetalleDto
    {
        public int Secuencia            { get; init; }
        public string Folio             { get; init; } = string.Empty;
        public string AliasCuenta       { get; init; } = string.Empty;
        public string CuentaContable    { get; init; } = string.Empty;
        public string DescripcionCuenta { get; init; } = string.Empty;
        public decimal Monto            { get; init; }
        public string Descripcion       { get; init; } = string.Empty;
        public string Proyecto          { get; init; } = string.Empty;
        public string CodUnidad1Cuenta  { get; init; } = string.Empty;
        public string CodUnidad3Cuenta  { get; init; } = string.Empty;
        public string CodUnidad4Cuenta  { get; init; } = string.Empty;
    }
}
