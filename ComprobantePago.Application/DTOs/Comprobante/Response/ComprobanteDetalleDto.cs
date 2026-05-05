namespace ComprobantePago.Application.DTOs.Comprobante.Response
{
    public class ComprobanteDetalleDto
    {
        // Encabezado
        public string Folio { get; set; } = null!;
        public string Ruc { get; set; }
        public string RazonSocial { get; set; }
        public string TipoDocumento { get; set; }
        public string TipoSunat { get; set; }
        public string Serie { get; set; }
        public string Numero { get; set; }
        public string FechaEmision { get; set; }
        public string FechaRecepcion { get; set; }
        public string Moneda { get; set; }
        public decimal TasaCambio { get; set; }
        public string CentroResponsabilidad { get; set; }
        public string DescripcionCR { get; set; }
        public string LugarPago { get; set; }
        public string PlazoPago { get; set; }
        public string FechaVencimiento { get; set; }
        public string RucBenef { get; set; }
        public string RazonSocialBenef { get; set; }
        public string Observacion { get; set; }
        public string OrdenCompra { get; set; }
        public bool FactMultiple { get; set; }
        public string EsDocumentoElectronico { get; set; }
        public string AplicaIGV { get; set; }
        public string Origen { get; set; }

        // Detracción
        public bool TieneDetraccion { get; set; }
        public string TipoDetraccion { get; set; }
        public decimal PorcentajeDetraccion { get; set; }
        public decimal MontoDetraccion { get; set; }
        public string ConstanciaDeposito { get; set; }
        public string FechaDeposito { get; set; }

        // Montos
        public string LblMontoNeto { get; set; }
        public decimal MontoNeto { get; set; }
        public string LblMontoExento { get; set; }
        public decimal MontoExento { get; set; }
        public string LblMontoIGVCosto { get; set; }
        public decimal MontoIGVCosto { get; set; }
        public decimal PorcentajeIGV { get; set; }
        public string LblMontoIGVCredito { get; set; }
        public decimal MontoIGVCredito { get; set; }
        public string LblMontoTotal { get; set; }
        public decimal MontoTotal { get; set; }
        public string LblMontoBruto { get; set; }
        public decimal MontoBruto { get; set; }
        public string LblMontoRetencion { get; set; }
        public decimal MontoRetencion { get; set; }
        public string LblMontoMultas { get; set; }
        public decimal MontoMultas { get; set; }
        public string LblValorAduana { get; set; }
        public decimal ValorAduana { get; set; }

        // Estado
        public string CodigoEstado { get; set; }
        public string Estado { get; set; }
        public string RolDigitacion { get; set; }
        public string FechaDigitacion { get; set; }
        public string RolAutorizacion { get; set; }
        public string FechaAutorizacion { get; set; }
        public string RolAprobacion { get; set; }
        public string FechaAprobacion { get; set; }
        public string RolAnulacion { get; set; }
        public string FechaAnulacion { get; set; }
        public string Mensaje { get; set; }
        // Empleado
        public bool EsEmpleado { get; set; }
        public string EmpleadoCodigo { get; set; } = string.Empty;
        public string EmpleadoNombre { get; set; } = string.Empty;

        // Flags
        public string RequiereAduana { get; set; }
        public string RequiereDetraccion { get; set; }
        public string IndicaValorReferencial { get; set; }
    }
}
