namespace ComprobantePago.Domain.Entities
{
    public class Comprobante
    {
        public int IdComprobante { get; set; }
        public string Folio { get; set; } = string.Empty;
        public string RucReceptor { get; set; } = string.Empty;
        public string RazonSocialReceptor { get; set; } = string.Empty;
        public string TipoDocumento { get; set; } = string.Empty;
        public string TipoSunat { get; set; } = string.Empty;
        public string Serie { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
        public DateTime? FechaRecepcion { get; set; }
        public string Moneda { get; set; } = string.Empty;
        public decimal TasaCambio { get; set; } = 1;
        public string? LugarPago { get; set; }
        public string? PlazoPago { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public string? RucBeneficiario { get; set; }
        public string? RazonSocialBenef { get; set; }
        public string? Observacion { get; set; }
        public string? OrdenCompra { get; set; }
        public bool FactMultiple { get; set; }
        public string? TipoDocAsociado { get; set; }
        public string? SerieAsociado { get; set; }
        public string? NumeroAsociado { get; set; }
        public bool TieneDetraccion { get; set; }
        public string? TipoDetraccion { get; set; }
        public decimal? PorcentajeDetraccion { get; set; }
        public decimal MontoDetraccion { get; set; }
        public string? ConstanciaDeposito { get; set; }
        public DateTime? FechaDeposito { get; set; }
        public decimal MontoNeto { get; set; }
        public decimal MontoExento { get; set; }
        public decimal PorcentajeIGV { get; set; }
        public decimal MontoIGVCosto { get; set; }
        public decimal MontoIGVCredito { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal MontoBruto { get; set; }
        public decimal MontoRetencion { get; set; }
        public decimal MontoMultas { get; set; }
        public decimal ValorAduana { get; set; }
        public decimal MontoRedondeo { get; set; }
        public bool EsDocumentoElectronico { get; set; } = true;
        public string AplicaIGV { get; set; } = "N";
        public string RequiereDetraccion { get; set; } = "N";
        public string RequiereAduana { get; set; } = "N";
        public bool TienePdf { get; set; }
        public bool EdicionManual { get; set; }
        public bool EstaDerivado { get; set; }
        public string CodigoEstado { get; set; } = "NUEVO";
        public string? EstadoSunat { get; set; }
        public string? RolDigitacion { get; set; }
        public DateTime? FechaDigitacion { get; set; }
        public string? RolAutorizacion { get; set; }
        public DateTime? FechaAutorizacion { get; set; }
        public string? RolAprobacion { get; set; }
        public DateTime? FechaAprobacion { get; set; }
        public string? RolAnulacion { get; set; }
        public DateTime? FechaAnulacion { get; set; }
        public string? Mensaje { get; set; }
        public string? Origen { get; set; }
        public string? SPO { get; set; }
        public string UsuarioReg { get; set; } = string.Empty;
        public DateTime FechaReg { get; set; } = DateTime.Now;
        public string? UsuarioAct { get; set; }
        public DateTime? FechaAct { get; set; }

        public bool EsEmpleado { get; set; }
        public string? EmpleadoCodigo { get; set; }
        public string? EmpleadoNombre { get; set; }
        public int? VoucherSyteline { get; set; }

        // Navegación
        public ICollection<ImputacionContable> Imputaciones { get; set; }
            = new List<ImputacionContable>();
    }
}
