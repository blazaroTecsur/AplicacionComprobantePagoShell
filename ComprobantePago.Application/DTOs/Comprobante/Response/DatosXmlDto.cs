namespace ComprobantePago.Application.DTOs.Comprobante.Response
{
    public class DatosXmlDto
    {
        // Receptor (empresa que recibe la factura)
        public string Ruc { get; set; }
        public string RazonSocial { get; set; }
        // Emisor (proveedor que emite la factura)
        public string RucProveedor { get; set; } = string.Empty;
        public string RazonSocialProveedor { get; set; } = string.Empty;
        // Datos del comprobante
        public string Serie { get; set; }
        public string Numero { get; set; }
        public string FechaEmision { get; set; }
        public string TipoDocumento { get; set; }
        public string TipoSunat { get; set; }
        public string Moneda { get; set; }
        public decimal TasaCambio { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal MontoNeto { get; set; }
        public decimal MontoIGV { get; set; }
        public decimal MontoExento { get; set; }
        public decimal MontoRetencion { get; set; }
        public decimal MontoIGVCredito { get; set; }
        public decimal MontoBruto { get; set; }
        public decimal PorcentajeIGV { get; set; }
        // ── Detracción ── NUEVO ───────────────────────────────────
        public bool TieneDetraccion { get; set; }
        public string CodigoDetraccion { get; set; } // ej: "030"
        public decimal PorcentajeDetraccion { get; set; } // ej: 4.00
        public decimal MontoDetraccion { get; set; } // ej: 466.00
        // ── Documento Asociado (Nota Crédito / Débito) ────────────
        public string TipoDocumentoAsociado { get; set; } = string.Empty; // ej: "01"
        public string SerieAsociado         { get; set; } = string.Empty; // ej: "F001"
        public string NumeroAsociado        { get; set; } = string.Empty; // ej: "00000123"
    }
}
