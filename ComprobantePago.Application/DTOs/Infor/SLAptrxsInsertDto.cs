namespace ComprobantePago.Application.DTOs.Infor
{
    /// <summary>
    /// Payload para insertar un comprobante aprobado en el IDO SLAptrxs de Infor Syteline.
    /// Mapea los campos de la cabecera del comprobante a los campos del IDO.
    /// </summary>
    public sealed class SLAptrxsInsertDto
    {
        // ── Campos requeridos por SLAptrxs ───────────────────────────────────

        /// <summary>
        /// Tipo de transacción AP. Campo Type (requerido).
        /// "Ajuste" para Notas de Crédito/Débito; "Comprobante" para los demás.
        /// </summary>
        public string Type { get; init; } = string.Empty;

        /// <summary>Código de proveedor Syteline (RUC o código empleado). Campo VendNum.</summary>
        public string VendNum { get; init; } = string.Empty;

        /// <summary>
        /// Número de voucher AP. Se envía con IsNull=true para que Syteline
        /// genere automáticamente el siguiente número de secuencia.
        /// </summary>
        public int Voucher { get; init; }

        /// <summary>Fecha de la factura. Campo InvDate.</summary>
        public string InvDate { get; init; } = string.Empty;

        /// <summary>Fecha de distribución. Campo DistDate.</summary>
        public string DistDate { get; init; } = string.Empty;

        /// <summary>Site destino. Campo UbToSite — se envía con IsNull=true.</summary>
        public string UbToSite { get; init; } = string.Empty;

        // ── Cabecera de la factura ────────────────────────────────────────────

        /// <summary>Número de factura (Serie-Numero). Campo InvNum.</summary>
        public string InvNum { get; init; } = string.Empty;

        /// <summary>Monto neto (base imponible). Campo PurchAmt.</summary>
        public decimal PurchAmt { get; init; }

        /// <summary>IGV / impuesto de venta. Campo SalesTax_2.</summary>
        public decimal SalesTax_2 { get; init; }

        /// <summary>Monto total de la factura. Campo InvAmt.</summary>
        public decimal InvAmt { get; init; }

        /// <summary>Cargos varios / monto exonerado. Campo MiscCharges.</summary>
        public decimal MiscCharges { get; init; }

        /// <summary>Monto sin descuento. Campo NonDiscAmt.</summary>
        public decimal NonDiscAmt { get; init; }

        // ── Vencimiento y descuento ───────────────────────────────────────────

        /// <summary>Fecha de vencimiento. Campo DueDate.</summary>
        public string DueDate { get; init; } = string.Empty;

        /// <summary>Días de vencimiento. Campo DueDays.</summary>
        public int DueDays { get; init; }

        /// <summary>Fecha de descuento. Campo DiscDate.</summary>
        public string DiscDate { get; init; } = string.Empty;

        /// <summary>Porcentaje de descuento. Campo DiscPct.</summary>
        public decimal DiscPct { get; init; }

        // ── Moneda ────────────────────────────────────────────────────────────

        /// <summary>Código de moneda (PEN/USD). Campo AptCurrCode.</summary>
        public string AptCurrCode { get; init; } = string.Empty;

        /// <summary>Tipo de cambio. Campo ExchRate.</summary>
        public decimal ExchRate { get; init; }

        // ── Cuenta A/P e imputación ───────────────────────────────────────────

        /// <summary>Cuenta contable A/P. Campo ApAcct.</summary>
        public string ApAcct { get; init; } = string.Empty;

        /// <summary>Código de unidad 1. Campo ApAcctUnit1.</summary>
        public string ApAcctUnit1 { get; init; } = string.Empty;

        /// <summary>Código de unidad 3. Campo ApAcctUnit3.</summary>
        public string ApAcctUnit3 { get; init; } = string.Empty;

        /// <summary>Código de unidad 4. Campo ApAcctUnit4.</summary>
        public string ApAcctUnit4 { get; init; } = string.Empty;

        // ── Referencia y notas ────────────────────────────────────────────────

        /// <summary>Referencia libre. Campo Ref (max 30 chars).</summary>
        public string Ref { get; init; } = string.Empty;

        /// <summary>Notas/memo de la transacción AP. Campo Txt (max 40 chars).</summary>
        public string Txt { get; init; } = string.Empty;

        /// <summary>Usuario autorizador. Campo Authorizer.</summary>
        public string Authorizer { get; init; } = string.Empty;

        /// <summary>
        /// Código de impuesto. Campo TaxCode1.
        /// "EXE" cuando el comprobante no tiene IGV o tiene monto exento;
        /// "NR" (No Recuperable) en caso contrario.
        /// </summary>
        public string TaxCode1 { get; init; } = string.Empty;

        // AuthStatus es un campo gestionado por Syteline — no se envía en additem.

        /// <summary>
        /// Estado de autorización. Campo AuthStatus.
        /// Siempre "Falló" al insertar desde el sistema externo.
        /// </summary>
        public string AuthStatus { get; init; } = string.Empty;

        /// <summary>Folio / ID del comprobante de origen. Campo aptZLA_SeqFac.</summary>
        public string aptZLA_SeqFac { get; init; } = string.Empty;

        // ── Detracción (campos locales Perú) ──────────────────────────────────

        /// <summary>Usa detracción: 1 = sí, 0 = no. Campo aptZLA_UsaDetraccion.</summary>
        public byte aptZLA_UsaDetraccion { get; init; }

        /// <summary>Código de detracción. Campo aptZLA_CodigoDetraccion.</summary>
        public string aptZLA_CodigoDetraccion { get; init; } = string.Empty;

        /// <summary>Tasa de detracción. Campo aptZLA_TasaDetraccion.</summary>
        public decimal aptZLA_TasaDetraccion { get; init; }

        /// <summary>Total detracción en moneda extranjera. Campo aptZLA_TotalDetraccion.</summary>
        public decimal aptZLA_TotalDetraccion { get; init; }

        /// <summary>Total detracción en moneda local. Campo aptZLA_TotalDetraccionLocal.</summary>
        public decimal aptZLA_TotalDetraccionLocal { get; init; }
    }
}
