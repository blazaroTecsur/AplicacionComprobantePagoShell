namespace ComprobantePago.Infrastructure.Services
{
    internal sealed class ComprobanteReporteData
    {
        // ── Empresa ───────────────────────────────
        public string EmpresaNombre  { get; set; } = string.Empty;
        public string EmpresaRuc     { get; set; } = string.Empty;

        // ── Cabecera ──────────────────────────────
        public string RucProveedor      { get; set; } = string.Empty;
        public string RazonSocial       { get; set; } = string.Empty;
        public string Serie             { get; set; } = string.Empty;
        public string Numero            { get; set; } = string.Empty;
        public string FechaEmision      { get; set; } = string.Empty;
        public string TipoDocumento     { get; set; } = string.Empty;
        public string TipoSunat         { get; set; } = string.Empty;
        public string DescTipoDocumento { get; set; } = string.Empty;
        public string Moneda            { get; set; } = string.Empty;
        public decimal TasaCambio       { get; set; }
        public string FechaRecepcion    { get; set; } = string.Empty;
        public string LugarPago         { get; set; } = string.Empty;
        public string DescLugarPago     { get; set; } = string.Empty;
        public string RucBenef          { get; set; } = string.Empty;
        public string OrdenCompra       { get; set; } = string.Empty;
        public string FechaVencimiento  { get; set; } = string.Empty;
        public bool   EsDocumentoElectronico { get; set; }
        public string EstadoSunat       { get; set; } = string.Empty;
        public string CodigoEstado      { get; set; } = string.Empty;

        // ── Montos ────────────────────────────────
        public decimal MontoNeto        { get; set; }
        public decimal MontoExento      { get; set; }
        public decimal PorcentajeIGV    { get; set; }
        public decimal MontoIGVCredito  { get; set; }
        public decimal MontoBruto       { get; set; }
        public decimal MontoRetencion   { get; set; }
        public decimal MontoDetraccion  { get; set; }

        // ── Detracción ────────────────────────────
        public bool    TieneDetraccion      { get; set; }
        public string  TipoDetraccion       { get; set; } = string.Empty;
        public decimal PorcentajeDetraccion { get; set; }

        // ── Flujo ─────────────────────────────────
        public string RolDigitacion   { get; set; } = string.Empty;
        public string FechaDigitacion { get; set; } = string.Empty;
        public string RolAutorizacion { get; set; } = string.Empty;
        public string Observacion     { get; set; } = string.Empty;

        // ── Imputaciones ──────────────────────────
        public List<ImputacionReporteData> Imputaciones { get; set; } = new();
    }

    internal sealed class ImputacionReporteData
    {
        public string  CuentaContable    { get; set; } = string.Empty;
        public string  CodUnidad1Cuenta  { get; set; } = string.Empty;
        public string  CodUnidad3Cuenta  { get; set; } = string.Empty;
        public string  CodUnidad4Cuenta  { get; set; } = string.Empty;
        public decimal Monto             { get; set; }
        public string  Descripcion       { get; set; } = string.Empty;
    }
}
