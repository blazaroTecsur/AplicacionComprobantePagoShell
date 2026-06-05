namespace ComprobantePago.Application.DTOs.Comprobante.Requests
{
    public class RegistrarComprobanteDto
    {
        public string Folio                  { get; set; } = string.Empty;
        public string Ruc                    { get; set; } = string.Empty;
        public string RazonSocial            { get; set; } = string.Empty;
        public string TipoDocumento          { get; set; } = string.Empty;
        public string TipoSunat              { get; set; } = string.Empty;
        public string Serie                  { get; set; } = string.Empty;
        public string Numero                 { get; set; } = string.Empty;
        public string FechaEmision           { get; set; } = string.Empty;
        public string FechaRecepcion         { get; set; } = string.Empty;
        public string Moneda                 { get; set; } = string.Empty;
        public decimal TasaCambio            { get; set; }
        public string CentroResponsabilidad  { get; set; } = string.Empty;
        public string LugarPago              { get; set; } = string.Empty;
        public string PlazoPago              { get; set; } = string.Empty;
        public string FechaVencimiento       { get; set; } = string.Empty;
        public string RucBenef               { get; set; } = string.Empty;
        public string Observacion            { get; set; } = string.Empty;
        public string OrdenCompra            { get; set; } = string.Empty;
        public bool FactMultiple             { get; set; }
        public bool TieneDetraccion          { get; set; }
        public string TipoDetraccion         { get; set; } = string.Empty;
        public decimal PorcentajeDetraccion  { get; set; }
        public decimal MontoDetraccion       { get; set; }
        public string ConstanciaDeposito     { get; set; } = string.Empty;
        public string FechaDeposito          { get; set; } = string.Empty;
        public string EsDocumentoElectronico { get; set; } = string.Empty;
        public string AplicaIGV              { get; set; } = string.Empty;
        public string Origen                 { get; set; } = string.Empty;
        // Montos
        public decimal MontoNeto             { get; set; }
        public decimal MontoExento           { get; set; }
        public decimal PorcentajeIGV         { get; set; }
        public decimal MontoIGVCredito       { get; set; }
        public decimal MontoTotal            { get; set; }
        public decimal MontoBruto            { get; set; }
        public decimal MontoRetencion        { get; set; }
        // Empleado
        public bool EsEmpleado               { get; set; }
        public string? EmpleadoCodigo        { get; set; }
        public string? EmpleadoNombre        { get; set; }
    }
}
