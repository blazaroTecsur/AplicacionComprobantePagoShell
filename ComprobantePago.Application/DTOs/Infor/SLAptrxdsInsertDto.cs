namespace ComprobantePago.Application.DTOs.Infor
{
    /// <summary>
    /// Payload para insertar una línea de distribución en el IDO SLAptrxds.
    /// Solo se incluyen los campos editables y significativos para el insert.
    /// </summary>
    public sealed class SLAptrxdsInsertDto
    {
        // ── Clave: vincula a la cabecera SLAptrxs ────────────────────────────

        /// <summary>Código de proveedor Syteline (7 chars). Campo VendNum (required).</summary>
        public string VendNum  { get; init; } = string.Empty;

        /// <summary>Número de voucher AP — mismo que la cabecera. Campo Voucher.</summary>
        public int Voucher     { get; init; }

        /// <summary>Secuencia de distribución (5, 10, 15…). Campo DistSeq.</summary>
        public int DistSeq     { get; init; }

        // ── Cuenta contable ───────────────────────────────────────────────────

        /// <summary>Cuenta contable (max 12). Campo Acct.</summary>
        public string Acct      { get; init; } = string.Empty;

        /// <summary>Unidad de análisis 1 (max 4). Campo AcctUnit1.</summary>
        public string AcctUnit1 { get; init; } = string.Empty;

        /// <summary>Unidad de análisis 3 (max 4). Campo AcctUnit3.</summary>
        public string AcctUnit3 { get; init; } = string.Empty;

        /// <summary>Unidad de análisis 4 (max 4). Campo AcctUnit4.</summary>
        public string AcctUnit4 { get; init; } = string.Empty;

        // ── Importe e impuesto ────────────────────────────────────────────────

        /// <summary>Monto de la línea. Campo Amount.</summary>
        public decimal Amount   { get; init; }

        /// <summary>Base imponible para el cálculo de impuesto. Campo TaxBasis.</summary>
        public decimal TaxBasis { get; init; }

        /// <summary>Código de impuesto principal (ej. "NR", "IGV18"). Campo TaxCode.</summary>
        public string TaxCode   { get; init; } = string.Empty;

        /// <summary>Código de impuesto exento (ej. "EXE"). Campo TaxCodeE.</summary>
        public string TaxCodeE  { get; init; } = string.Empty;

        /// <summary>Sistema de impuesto: "1"=NR, "2"=IGV. Campo TaxSystem.</summary>
        public string TaxSystem { get; init; } = string.Empty;

        // ── DIOT y número fiscal ──────────────────────────────────────────────

        /// <summary>Tipo de operación DIOT (ej. "1"=compra local con IGV). Campo DIOTTransType.</summary>
        public string DIOTTransType { get; init; } = string.Empty;

        /// <summary>RUC / número fiscal del proveedor/beneficiario. Campo TaxRegNum.</summary>
        public string TaxRegNum     { get; init; } = string.Empty;

        /// <summary>Tipo de registro fiscal: "T"=RUC. Campo TaxRegNumType.</summary>
        public string TaxRegNumType { get; init; } = string.Empty;

        // ── Proyecto ──────────────────────────────────────────────────────────

        /// <summary>Número de proyecto (max 10). Campo ProjNum.</summary>
        public string ProjNum { get; init; } = string.Empty;

        // ── Campos extendidos Perú ─────────────────────────────────────────────

        /// <summary>
        /// VendNum del proveedor real pagado por el empleado (7 chars, padded).
        /// Solo se rellena en la línea principal de comprobantes de empleado.
        /// Campo aptZCO_APD_VendNum.
        /// </summary>
        public string aptZCO_APD_VendNum { get; init; } = string.Empty;

        /// <summary>Tipo de documento SUNAT (ej. "01"=Factura). Campo aptZLA_TipoDocumento.</summary>
        public string aptZLA_TipoDocumento { get; init; } = string.Empty;

        /// <summary>Nombre/razón social del proveedor o empleado. Campo VendorName.</summary>
        public string VendorName { get; init; } = string.Empty;

        /// <summary>"0" para empleados; vacío (no se envía) para proveedores. Campo ForeignTaxRegNum.</summary>
        public string ForeignTaxRegNum { get; init; } = string.Empty;
    }
}
