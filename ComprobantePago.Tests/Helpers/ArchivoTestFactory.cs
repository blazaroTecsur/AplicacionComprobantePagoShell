using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using System.IO.Compression;
using System.Text;

namespace ComprobantePago.Tests.Helpers
{
    /// <summary>
    /// Fábrica de archivos de prueba (IFormFile) para simular
    /// cargas de XML, PDF y ZIP de comprobantes SUNAT.
    /// </summary>
    public static class ArchivoTestFactory
    {
        // ── XML mínimo válido UBL 2.1 (SUNAT Factura) ────────────────────────
        public static string XmlFacturaSunat(
            string ruc         = "20206018411",
            string serie       = "F001",
            string numero      = "00000001",
            string fecha       = "2026-03-01",
            string moneda      = "PEN",
            decimal montoTotal = 1180.00m,
            string tipoDoc     = "01")
        {
            return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
                <Invoice xmlns=""urn:oasis:names:specification:ubl:schema:xsd:Invoice-2""
                         xmlns:cbc=""urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2""
                         xmlns:cac=""urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2"">
                  <cbc:ID>{serie}-{numero}</cbc:ID>
                  <cbc:IssueDate>{fecha}</cbc:IssueDate>
                  <cbc:InvoiceTypeCode>{tipoDoc}</cbc:InvoiceTypeCode>
                  <cbc:DocumentCurrencyCode>{moneda}</cbc:DocumentCurrencyCode>
                  <cac:AccountingSupplierParty>
                    <cac:Party>
                      <cac:PartyIdentification>
                        <cbc:ID>{ruc}</cbc:ID>
                      </cac:PartyIdentification>
                      <cac:PartyLegalEntity>
                        <cbc:RegistrationName>EMPRESA EMISORA SAC</cbc:RegistrationName>
                      </cac:PartyLegalEntity>
                    </cac:Party>
                  </cac:AccountingSupplierParty>
                  <cac:LegalMonetaryTotal>
                    <cbc:PayableAmount>{montoTotal.ToString(System.Globalization.CultureInfo.InvariantCulture)}</cbc:PayableAmount>
                  </cac:LegalMonetaryTotal>
                </Invoice>";
        }

        // ── IFormFile a partir de bytes ───────────────────────────────────────
        public static IFormFile CrearFormFile(byte[] contenido, string nombreArchivo, string contentType = "application/octet-stream")
        {
            var stream = new MemoryStream(contenido);
            return new FormFile(stream, 0, contenido.Length, "archivo", nombreArchivo)
            {
                Headers     = new HeaderDictionary(),
                ContentType = contentType
            };
        }

        // ── IFormFile XML ─────────────────────────────────────────────────────
        public static IFormFile CrearFormFileXml(string xmlContent, string nombreArchivo = "factura.xml")
        {
            var bytes = Encoding.UTF8.GetBytes(xmlContent);
            return CrearFormFile(bytes, nombreArchivo, "text/xml");
        }

        // ── IFormFile ZIP (XML + CDR R-*.zip) ────────────────────────────────
        public static IFormFile CrearFormFileZip(
            string xmlContent,
            string nombreXml   = "F001-00000001.xml",
            bool   incluirCdr  = true,
            string? nombreCdr  = null)
        {
            using var zipMs = new MemoryStream();
            using (var zip = new ZipArchive(zipMs, ZipArchiveMode.Create, leaveOpen: true))
            {
                // XML principal
                var entradaXml = zip.CreateEntry(nombreXml);
                using (var writer = new StreamWriter(entradaXml.Open()))
                    writer.Write(xmlContent);

                // CDR simulado (R-*.zip vacío)
                if (incluirCdr)
                {
                    var cdrNombre = nombreCdr ?? $"R-{nombreXml.Replace(".xml", ".zip")}";
                    using var cdrMs = new MemoryStream();
                    using (var cdrZip = new ZipArchive(cdrMs, ZipArchiveMode.Create, leaveOpen: true))
                    {
                        var xmlCdr = cdrZip.CreateEntry("cdr.xml");
                        using var w = new StreamWriter(xmlCdr.Open());
                        w.Write("<CDR/>");
                    }
                    var entradaCdr = zip.CreateEntry(cdrNombre);
                    using var es = entradaCdr.Open();
                    cdrMs.Position = 0;
                    cdrMs.CopyTo(es);
                }
            }

            zipMs.Position = 0;
            return CrearFormFile(zipMs.ToArray(), "comprobante.zip", "application/zip");
        }
    }
}
