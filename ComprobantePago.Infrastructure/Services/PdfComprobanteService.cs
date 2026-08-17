using ComprobantePago.Application.DTOs.Comprobante.Response;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;

namespace ComprobantePago.Infrastructure.Services
{
    public class PdfComprobanteService
    {
        public async Task<string> ExtraerTextoPdfAsync(IFormFile archivo)
        {
            using var stream = archivo.OpenReadStream();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);

            var sb = new StringBuilder();
            using var doc = PdfDocument.Open(ms.ToArray());
            foreach (var pagina in doc.GetPages())
                sb.AppendLine(pagina.Text);

            return sb.ToString();
        }

        /// Extrae los datos del PDF en un DatosXmlDto.
        /// Regex validados contra el texto real extraído por PdfPig
        public DatosXmlDto ExtraerDatosPdf(string texto)
        {
            try
            {
                var datos = new DatosXmlDto();

                // ── Serie y Número ────────────────────────────────────
                // Texto: "...FACTURAF557-00183136Lima..."
                var matchSerie = Regex.Match(texto, @"\b([A-Z]\d{3})-(\d{8})\b");
                if (matchSerie.Success)
                {
                    datos.Serie = matchSerie.Groups[1].Value;
                    datos.Numero = matchSerie.Groups[2].Value;
                }

                // ── RUC Emisor (proveedor que emite) ──────────────────
                // Texto: "R.U.C.ELECTRÓNICA20206018411FACTURA..."
                var matchRucEmisor = Regex.Match(texto,
                    @"R\.U\.C\.(?:ELECTR[OÓ]NICA)?(\d{11})");
                if (matchRucEmisor.Success)
                    datos.RucProveedor = matchRucEmisor.Groups[1].Value;

                // ── RUC Receptor (empresa que recibe) ─────────────────
                // Texto: "...Dirección :20331898008Categoría:OTROS..."
                // El RUC del cliente aparece entre "Dirección :" y "Categoría"
                var matchRucReceptor = Regex.Match(texto,
                    @"Direcci[oó]n\s*:\s*(?:\d+)?(\d{11})Categor[ií]a");
                if (matchRucReceptor.Success)
                    datos.Ruc = matchRucReceptor.Groups[1].Value;

                // ── Razón Social Receptor ─────────────────────────────
                // Texto: "...Cliente :CR LDS: 60244Moneda :LUZ DEL SUR S.A.A.Obs...."
                // Extraer entre "Cliente :" y "Obs.", luego limpiar prefijo hasta "Moneda :"
                var matchCliente = Regex.Match(texto,
                    @"Cliente\s*:(.+?)Obs\.");
                if (matchCliente.Success)
                {
                    var raw = matchCliente.Groups[1].Value.Trim();
                    // Quitar "CR LDS: 60244Moneda :" o similar hasta la razón social
                    var cleanMatch = Regex.Match(raw, @"Moneda\s*:(.+)$");
                    datos.RazonSocial = cleanMatch.Success
                        ? cleanMatch.Groups[1].Value.Trim()
                        : raw;
                }

                // ── Razón Social Emisor ───────────────────────────────
                // Texto: "...2025.TECSUR S.A.Pasaje Calango..."
                // Aparece entre la fecha (termina en ".") y "Pasaje" / dirección
                var matchEmisor = Regex.Match(texto,
                    @"\d{4}\.\s*(.+?)(?:Pasaje|Av\.|Jr\.|Cal\.|Calle|Urb\.)");
                if (matchEmisor.Success)
                    datos.RazonSocialProveedor = matchEmisor.Groups[1].Value.Trim();

                // ── Fecha de Emisión ──────────────────────────────────
                // Texto: "Lima, 01 de Mayo de 2025."
                var matchFecha = Regex.Match(texto,
                    @"(\d{1,2})\s+de\s+(\w+)\s+de\s+(\d{4})",
                    RegexOptions.IgnoreCase);
                if (matchFecha.Success)
                    datos.FechaEmision = ConvertirFecha(
                        matchFecha.Groups[1].Value,
                        matchFecha.Groups[2].Value,
                        matchFecha.Groups[3].Value
                    );

                // ── Moneda ────────────────────────────────────────────
                // Texto: "...Moneda :LUZ DEL SUR S.A.A....SolCR Ventas..."
                // La moneda "Sol" aparece entre el bloque de datos y "CR Ventas"
                var matchMoneda = Regex.Match(texto,
                    @"(?:Moneda\s*:.+?)(Sol|Dolar|Soles|Dolares)(?:CR|Cond\.)",
                    RegexOptions.IgnoreCase);
                datos.Moneda = matchMoneda.Success
                    ? matchMoneda.Groups[1].Value.Trim().ToUpper() switch
                    {
                        "SOL" => "PEN",
                        "SOLES" => "PEN",
                        "DOLAR" => "USD",
                        "DOLARES" => "USD",
                        _ => "PEN"
                    }
                    : "PEN";

                // ── Tipo Documento ────────────────────────────────────
                if (Regex.IsMatch(texto, @"FACTURA", RegexOptions.IgnoreCase)) datos.TipoDocumento = "01";
                else if (Regex.IsMatch(texto, @"BOLETA", RegexOptions.IgnoreCase)) datos.TipoDocumento = "03";
                else if (Regex.IsMatch(texto, @"NOTA DE CR[EÉ]DITO", RegexOptions.IgnoreCase)) datos.TipoDocumento = "07";
                else if (Regex.IsMatch(texto, @"NOTA DE D[EÉ]BITO", RegexOptions.IgnoreCase)) datos.TipoDocumento = "08";

                datos.TipoSunat = datos.TipoDocumento;

                // ── Montos ────────────────────────────────────────────
                // Texto: "...TOTAL FACT.:11,659.759,881.1418.00%1,778.61..."
                // Los 3 montos aparecen concatenados: Total, Neto, IGV%
                var matchMontos = Regex.Match(texto,
                    @"TOTAL FACT\.:?\s*([\d,]+\.\d{2})([\d,]+\.\d{2})[\d.]+%([\d,]+\.\d{2})");
                if (matchMontos.Success)
                {
                    datos.MontoTotal = ParsearMonto(matchMontos.Groups[1].Value);
                    datos.MontoNeto = ParsearMonto(matchMontos.Groups[2].Value);
                    datos.MontoIGV = ParsearMonto(matchMontos.Groups[3].Value);
                }

                return datos;
            }
            catch
            {
                return null;
            }
        }

        /// Convierte fecha en texto español a formato dd/MM/yyyy.
        /// Ejemplo: "1", "Mayo", "2025" → "01/05/2025"
        private string ConvertirFecha(string dia, string mes, string anio)
        {
            var meses = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["enero"] = "01",
                ["febrero"] = "02",
                ["marzo"] = "03",
                ["abril"] = "04",
                ["mayo"] = "05",
                ["junio"] = "06",
                ["julio"] = "07",
                ["agosto"] = "08",
                ["septiembre"] = "09",
                ["octubre"] = "10",
                ["noviembre"] = "11",
                ["diciembre"] = "12"
            };
            var numMes = meses.TryGetValue(mes, out var m) ? m : "01";
            return $"{dia.PadLeft(2, '0')}/{numMes}/{anio}";
        }

        /// Parsea un monto con comas como separador de miles.
        /// Ejemplo: "11,659.75" → 11659.75
        private decimal ParsearMonto(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return 0;
            return decimal.TryParse(
                valor.Replace(",", ""),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var result) ? result : 0;
        }
    }
}
