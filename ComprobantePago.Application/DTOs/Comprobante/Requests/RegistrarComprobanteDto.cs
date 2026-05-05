namespace ComprobantePago.Application.DTOs.Comprobante.Requests
{
    public class RegistrarComprobanteDto
    {
        public string Folio { get; set; } = null!;
        public string Ruc { get; set; } = null!;
        public string RazonSocial { get; set; } = null!;
        public string TipoDocumento { get; set; } = null!;
        public string TipoSunat { get; set; } = null!;
        public string Serie { get; set; } = null!;
        public string Numero { get; set; } = null!;
        public string FechaEmision { get; set; } = null!;
        public string FechaRecepcion { get; set; } = null!;
        public string Moneda { get; set; } = null!;
        public decimal TasaCambio { get; set; }
        public string CentroResponsabilidad { get; set; } = null!;
        public string LugarPago { get; set; } = null!;
        public string PlazoPago { get; set; } = null!;
        public string FechaVencimiento { get; set; } = null!;
        public string RucBenef { get; set; } = null!;
        public string Observacion { get; set; } = null!;
        public string OrdenCompra { get; set; } = null!;
        public bool FactMultiple { get; set; } 
        public bool TieneDetraccion { get; set; } 
        public string TipoDetraccion { get; set; } = null!;
        public decimal PorcentajeDetraccion { get; set; }
        public decimal MontoDetraccion { get; set; }
        public string ConstanciaDeposito { get; set; } = null!;
        public string FechaDeposito { get; set; } = null!;
        public string EsDocumentoElectronico { get; set; } = null!;
        public string AplicaIGV { get; set; } = null!;
        public string Origen { get; set; } = null!;
        // Montos
        public decimal MontoNeto { get; set; }
        public decimal MontoExento { get; set; }
        public decimal PorcentajeIGV { get; set; }
        public decimal MontoIGVCredito { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal MontoBruto { get; set; }
        public decimal MontoRetencion { get; set; }
        // Empleado
        public bool EsEmpleado { get; set; }
        public string? EmpleadoCodigo { get; set; }
        public string? EmpleadoNombre { get; set; }
    }
}
