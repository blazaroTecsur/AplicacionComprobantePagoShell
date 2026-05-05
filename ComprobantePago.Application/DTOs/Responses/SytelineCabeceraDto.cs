namespace ComprobantePago.Application.DTOs.Responses
{
    public sealed class SytelineCabeceraDto
    {
        /// <summary>RUC o código del proveedor (para visualización y export Excel).</summary>
        public string Proveedor          { get; init; } = string.Empty;

        /// <summary>
        /// VendNum de Syteline — viene de tmaproveedor.IdProveedorExternal.
        /// Es el campo que se envía al IDO SLAptrxs. Vacío si el proveedor
        /// no está sincronizado con Syteline.
        /// </summary>
        public string VendNum            { get; init; } = string.Empty;

        public string Nombre             { get; init; } = string.Empty;
        public int Comprobante           { get; init; }
        /// <summary>Número de voucher Syteline pre-asignado para el Excel (max+n). 0 si no fue asignado.</summary>
        public int VoucherSyteline       { get; set; }
        /// <summary>Folio del comprobante en el sistema (clave para DerivarAsync).</summary>
        public string Folio              { get; init; } = string.Empty;
        public string Factura            { get; init; } = string.Empty;
        public string FechaFactura       { get; init; } = string.Empty;
        public string FechaDistribucion  { get; init; } = string.Empty;
        public decimal ImpoCompra        { get; init; }
        public decimal CargosVarios      { get; init; }
        public decimal ImpVentas2        { get; init; }
        public decimal MntoFactura       { get; init; }
        public decimal ImpSinDesc        { get; init; }
        public string FechaDcto          { get; init; } = string.Empty;
        public int DiasVto               { get; init; }
        public string FechaVen           { get; init; } = string.Empty;
        public string Moneda             { get; init; } = string.Empty;
        public decimal TipoCambio        { get; init; }
        public string CtaCP              { get; init; } = string.Empty;
        public string CtaCPUnid1         { get; init; } = string.Empty;
        public string CtaCPUnid2         { get; init; } = string.Empty;
        public string CtaCPUnid3         { get; init; } = string.Empty;
        public string CtaCPUnid4         { get; init; } = string.Empty;
        public string DescripcionCuenta  { get; init; } = string.Empty;
        public string Ref                { get; init; } = string.Empty;
        public string EstadoAut          { get; init; } = string.Empty;
        public string Autorizo           { get; init; } = string.Empty;
        public string Notas              { get; init; } = string.Empty;
        public string UsaDetraccion      { get; init; } = string.Empty;
        public string Detraccion         { get; init; } = string.Empty;
        public decimal Tasa              { get; init; }
        public decimal TotalDetraccion   { get; init; }
        public decimal TotalDetLocal     { get; init; }
        public decimal MontoExento       { get; init; }
        public decimal MontoRetencion    { get; init; }
        public decimal PorcentajeIGV     { get; init; }
        /// <summary>Código de tipo SUNAT: "07"=Nota Crédito, "08"=Nota Débito, otros=Comprobante.</summary>
        public string TipoSunat          { get; init; } = string.Empty;
    }
}
