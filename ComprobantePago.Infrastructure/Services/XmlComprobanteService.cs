using ComprobantePago.Application.DTOs.Comprobante.Response;
using System.Xml;
using System.Xml.Linq;

namespace ComprobantePago.Infrastructure.Services
{
    public class XmlComprobanteService
    {
        // Namespaces del XML de SUNAT
        private static readonly XNamespace cbc =
            "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
        private static readonly XNamespace cac =
            "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
        private static readonly XNamespace ext =
            "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2";
        private static readonly XNamespace biz =
            "urn:bizlinks:names:specification:ubl:peru:schema:xsd:BizlinksAggregateComponents-1";

        // Configuración segura: deshabilita DTD y entidades externas (previene XXE)
        private static readonly XmlReaderSettings _xmlSettings = new()
        {
            DtdProcessing             = DtdProcessing.Prohibit,
            XmlResolver               = null,
            MaxCharactersFromEntities = 0
        };

        public DatosXmlDto LeerDatosXml(Stream stream)
        {
            using var reader = XmlReader.Create(stream, _xmlSettings);
            var xml = XDocument.Load(reader);
            var root = xml.Root;

            var datos = new DatosXmlDto();

            try
            {
                // Serie y Número
                var id = root?.Element(cbc + "ID")?.Value ?? "";
                if (id.Contains("-"))
                {
                    datos.Serie = id.Split('-')[0];
                    datos.Numero = id.Split('-')[1];
                }

                // Fecha emisión
                var fechaRaw = root?
                    .Element(cbc + "IssueDate")?.Value ?? "";

                // Convertir de yyyy-MM-dd a dd/MM/yyyy
                if (DateTime.TryParse(fechaRaw, out var fecha))
                {
                    datos.FechaEmision = fecha.ToString("dd/MM/yyyy");
                }
                else
                {
                    datos.FechaEmision = fechaRaw;
                }

                // Tipo documento (código SUNAT)
                // - Invoice    → InvoiceTypeCode   (01 = Factura, 03 = Boleta, etc.)
                // - CreditNote → CreditNoteTypeCode o fallback "07"
                // - DebitNote  → DebitNoteTypeCode  o fallback "08"
                var rootName = root?.Name.LocalName ?? "";
                datos.TipoSunat = rootName switch
                {
                    "CreditNote" => root!.Element(cbc + "CreditNoteTypeCode")?.Value is { Length: > 0 } v1 ? v1 : "07",
                    "DebitNote"  => root!.Element(cbc + "DebitNoteTypeCode")?.Value  is { Length: > 0 } v2 ? v2 : "08",
                    _            => root?.Element(cbc + "InvoiceTypeCode")?.Value ?? ""
                };

                // Moneda
                datos.Moneda = root?
                    .Element(cbc + "DocumentCurrencyCode")?.Value ?? "";

                // Tipo de cambio: PEN/soles siempre es 1; otras monedas se leen del XML
                if (datos.Moneda.Equals("PEN", StringComparison.OrdinalIgnoreCase))
                {
                    datos.TasaCambio = 1m;
                }
                else
                {
                    var tcRaw = root?
                        .Element(cac + "PaymentExchangeRate")?
                        .Element(cbc + "CalculationRate")?.Value ?? "";
                    datos.TasaCambio = decimal.TryParse(
                        tcRaw,
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out var tc) ? tc : 0m;
                }

                // También leer el proveedor (emisor) por separado
                var supplier = root?.Element(cac + "AccountingSupplierParty");
                var partySupplier = supplier?.Element(cac + "Party");

                datos.RucProveedor = partySupplier?
                    .Element(cac + "PartyIdentification")?
                    .Element(cbc + "ID")?.Value ?? string.Empty;

                datos.RazonSocialProveedor = partySupplier?
                    .Element(cac + "PartyLegalEntity")?
                    .Element(cbc + "RegistrationName")?.Value ?? string.Empty;

                // RUC y Razón Social del proveedor (emisor)
                var customer = root?
                    .Element(cac + "AccountingCustomerParty");
                var partyCustomer = customer?
                    .Element(cac + "Party");

                datos.Ruc = partyCustomer?
                    .Element(cac + "PartyIdentification")?
                    .Element(cbc + "ID")?.Value ?? "";

                datos.RazonSocial = partyCustomer?
                    .Element(cac + "PartyLegalEntity")?
                    .Element(cbc + "RegistrationName")?.Value ?? "";

                // Montos
                var monetaryTotal = root?
                    .Element(cac + "LegalMonetaryTotal");

                var totalStr = monetaryTotal?
                    .Element(cbc + "PayableAmount")?.Value ?? "0";
                datos.MontoTotal = decimal.TryParse(
                    totalStr,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var total) ? total : 0;

                var netoStr = monetaryTotal?
                    .Element(cbc + "LineExtensionAmount")?.Value;

                if (decimal.TryParse(
                        netoStr,
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out var neto) && neto > 0)
                {
                    datos.MontoNeto = neto;
                }
                // IGV  (se calcula antes de MontoNeto porque el fallback lo necesita)
                var taxTotal = root?
                    .Element(cac + "TaxTotal");
                var igvStr = taxTotal?
                    .Element(cbc + "TaxAmount")?.Value ?? "0";
                datos.MontoIGV = decimal.TryParse(
                    igvStr,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var igv) ? igv : 0;

                if (!decimal.TryParse(
                        netoStr,
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out neto) || neto == 0)
                {
                    // CreditNote / DebitNote: LineExtensionAmount no existe en LegalMonetaryTotal.
                    // MontoNeto = PayableAmount - TaxTotal/TaxAmount
                    datos.MontoNeto = datos.MontoTotal - datos.MontoIGV;
                }

                // MontoExento: TaxSubtotal con TaxScheme "9997" (Exonerado) o "9998" (Inafecto)
                datos.MontoExento = root?
                    .Descendants(cac + "TaxSubtotal")
                    .Where(ts => ts.Descendants(cbc + "ID")
                        .Any(id => id.Value == "9997" || id.Value == "9998"))
                    .Sum(ts => decimal.TryParse(
                        ts.Element(cbc + "TaxableAmount")?.Value ?? "0",
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out var v) ? v : 0) ?? 0;

                // MontoRetencion: TaxSubtotal con TaxScheme "2000" o "2001"
                datos.MontoRetencion = root?
                    .Descendants(cac + "TaxSubtotal")
                    .Where(ts => ts.Descendants(cbc + "ID")
                        .Any(id => id.Value == "2000" || id.Value == "2001"))
                    .Sum(ts => decimal.TryParse(
                        ts.Element(cbc + "TaxAmount")?.Value ?? "0",
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out var v) ? v : 0) ?? 0;

                // MontoIGVCredito y PorcentajeIGV: TaxSubtotal con TaxScheme "1000" (IGV gravado)
                var taxSubtotalIGV = root?
                    .Descendants(cac + "TaxSubtotal")
                    .FirstOrDefault(ts => ts.Descendants(cbc + "ID")
                        .Any(id => id.Value == "1000"));

                datos.MontoIGVCredito = taxSubtotalIGV != null && decimal.TryParse(
                    taxSubtotalIGV.Element(cbc + "TaxAmount")?.Value ?? "0",
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var igvCred) ? igvCred : 0;

                // PorcentajeIGV: AdditionalProperty con ID "9163" (ej: "18.00%")
                var pctRaw = root?
                    .Descendants(biz + "AdditionalProperty")
                    .FirstOrDefault(ap => ap.Element(cbc + "ID")?.Value == "9163")?
                    .Element(cbc + "Value")?.Value ?? "";
                datos.PorcentajeIGV = decimal.TryParse(
                    pctRaw.Replace("%", "").Trim(),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var pctIGV) ? pctIGV : 0;

                // MontoBruto = MontoTotal - MontoRetencion
                datos.MontoBruto = datos.MontoTotal - datos.MontoRetencion;

                // Detracción
                // El XML tiene dos <cac:PaymentTerms>:
                //   - ID = "Detraccion"  → tiene PaymentPercent y Amount
                //   - ID = "FormaPago"   → forma de pago, ignorar
                var paymentTerms = root?
                    .Elements(cac + "PaymentTerms")
                    .FirstOrDefault(pt =>
                        pt.Element(cbc + "ID")?.Value == "Detraccion");

                if (paymentTerms != null)
                {
                    datos.TieneDetraccion = true;

                    // Código de detracción (ej: "030" = Contratos de Construcción)
                    datos.CodigoDetraccion = paymentTerms
                        .Element(cbc + "PaymentMeansID")?.Value ?? string.Empty;

                    // Porcentaje (ej: "4.00")
                    var pctStr = paymentTerms
                        .Element(cbc + "PaymentPercent")?.Value ?? "0";
                    datos.PorcentajeDetraccion = decimal.TryParse(
                        pctStr,
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out var pctd) ? pctd : 0;

                    // Monto (ej: "466.00")
                    var montoDetStr = paymentTerms
                        .Element(cbc + "Amount")?.Value ?? "0";
                    datos.MontoDetraccion = decimal.TryParse(
                        montoDetStr,
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out var montoDet) ? montoDet : 0;
                }

                // Documento Asociado — solo en CreditNote y DebitNote
                // BillingReference/InvoiceDocumentReference contiene Serie-Numero y tipo del doc original
                var billingRef = root?.Element(cac + "BillingReference")?
                    .Element(cac + "InvoiceDocumentReference");

                if (billingRef != null)
                {
                    var refId = billingRef.Element(cbc + "ID")?.Value ?? "";
                    if (refId.Contains('-'))
                    {
                        datos.SerieAsociado  = refId.Split('-')[0];
                        datos.NumeroAsociado = refId.Split('-')[1];
                    }
                    else
                    {
                        datos.NumeroAsociado = refId;
                    }
                    datos.TipoDocumentoAsociado = billingRef
                        .Element(cbc + "DocumentTypeCode")?.Value ?? "";
                }
            }
            catch
            {
                // Si falla la lectura devuelve lo que pudo leer
            }

            return datos;
        }
    }
}
