namespace ComprobantePago.Application.DTOs.Comprobante.Response
{
    public class ComprobanteDetalleDto
    {
        // Encabezado
        public string Folio               { get; set; } = string.Empty;
        public string Ruc                 { get; set; } = string.Empty;
        public string RazonSocial         { get; set; } = string.Empty;
        public string TipoDocumento       { get; set; } = string.Empty;
        public string TipoSunat           { get; set; } = string.Empty;
        public string Serie               { get; set; } = string.Empty;
        public string Numero              { get; set; } = string.Empty;
        public string FechaEmision        { get; set; } = string.Empty;
        public string FechaRecepcion      { get; set; } = string.Empty;
        public string Moneda              { get; set; } = string.Empty;
        public decimal TasaCambio         { get; set; }
        public string CentroResponsabilidad { get; set; } = string.Empty;
        public string DescripcionCR       { get; set; } = string.Empty;
        public string LugarPago           { get; set; } = string.Empty;
        public string PlazoPago           { get; set; } = string.Empty;
        public string FechaVencimiento    { get; set; } = string.Empty;
        public string RucBenef            { get; set; } = string.Empty;
        public string RazonSocialBenef    { get; set; } = string.Empty;
        public string Observacion         { get; set; } = string.Empty;
        public string OrdenCompra         { get; set; } = string.Empty;
        public bool FactMultiple          { get; set; }
        public string EsDocumentoElectronico { get; set; } = string.Empty;
        public string AplicaIGV           { get; set; } = string.Empty;
        public string Origen              { get; set; } = string.Empty;

        // Detracción
        public bool TieneDetraccion       { get; set; }
        public string TipoDetraccion      { get; set; } = string.Empty;
        public decimal PorcentajeDetraccion { get; set; }
        public decimal MontoDetraccion    { get; set; }
        public string ConstanciaDeposito  { get; set; } = string.Empty;
        public string FechaDeposito       { get; set; } = string.Empty;

        // Montos
        public string LblMontoNeto        { get; set; } = string.Empty;
        public decimal MontoNeto          { get; set; }
        public string LblMontoExento      { get; set; } = string.Empty;
        public decimal MontoExento        { get; set; }
        public string LblMontoIGVCosto    { get; set; } = string.Empty;
        public decimal MontoIGVCosto      { get; set; }
        public decimal PorcentajeIGV      { get; set; }
        public string LblMontoIGVCredito  { get; set; } = string.Empty;
        public decimal MontoIGVCredito    { get; set; }
        public string LblMontoTotal       { get; set; } = string.Empty;
        public decimal MontoTotal         { get; set; }
        public string LblMontoBruto       { get; set; } = string.Empty;
        public decimal MontoBruto         { get; set; }
        public string LblMontoRetencion   { get; set; } = string.Empty;
        public decimal MontoRetencion     { get; set; }
        public string LblMontoMultas      { get; set; } = string.Empty;
        public decimal MontoMultas        { get; set; }
        public string LblValorAduana      { get; set; } = string.Empty;
        public decimal ValorAduana        { get; set; }

        // Estado
        public string CodigoEstado        { get; set; } = string.Empty;
        public string Estado              { get; set; } = string.Empty;
        public string RolDigitacion       { get; set; } = string.Empty;
        public string FechaDigitacion     { get; set; } = string.Empty;
        public string RolAutorizacion     { get; set; } = string.Empty;
        public string FechaAutorizacion   { get; set; } = string.Empty;
        public string RolAprobacion       { get; set; } = string.Empty;
        public string FechaAprobacion     { get; set; } = string.Empty;
        public string RolAnulacion        { get; set; } = string.Empty;
        public string FechaAnulacion      { get; set; } = string.Empty;
        public string Mensaje             { get; set; } = string.Empty;

        // Empleado
        public bool EsEmpleado            { get; set; }
        public string EmpleadoCodigo      { get; set; } = string.Empty;
        public string EmpleadoNombre      { get; set; } = string.Empty;

        // Flags
        public string RequiereAduana          { get; set; } = string.Empty;
        public string RequiereDetraccion      { get; set; } = string.Empty;
        public string IndicaValorReferencial  { get; set; } = string.Empty;
    }
}
