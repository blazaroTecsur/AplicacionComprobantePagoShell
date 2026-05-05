using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ComprobantePago.Infrastructure.Services
{
    internal static class ComprobantePdfReporteService
    {
        // Separadores — ancho calculado para Letter (612 pt) con márgenes 1.2 cm c/lado (≈ 34 pt):
        // usable ≈ 544 pt · Courier New 7.5 pt ≈ 4.5 pt/car → ~121 caracteres.
        private static readonly string SEP_IGUAL = new string('=', 121);
        private static readonly string SEP_GUION = new string('-', 121);

        static ComprobantePdfReporteService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public static byte[] Generar(ComprobanteReporteData d)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(1.2f, Unit.Centimetre);
                    page.DefaultTextStyle(t => t
                        .FontFamily("Courier New")
                        .FontSize(7.5f)
                        .FontColor(Colors.Black));

                    page.Content().Column(col =>
                    {
                        col.Spacing(0);

                        // ── ENCABEZADO ─────────────────────────────
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text($"Empresa : {d.EmpresaNombre}");
                            row.AutoItem().Text("Página :  1");
                        });
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text(d.EmpresaRuc);
                            row.RelativeItem().AlignCenter()
                               .Text("COMPROBANTE LIBRO COMPRAS HONORARIOS")
                               .Bold();
                        });
                        col.Item().Text(SEP_IGUAL);

                        // ── PROVEEDOR ──────────────────────────────
                        col.Item().PaddingTop(4).Row(row =>
                        {
                            row.AutoItem().Text("RUC     : ");
                            row.AutoItem().Text(d.RucProveedor);
                            row.AutoItem().PaddingLeft(6).Text(d.RazonSocial);
                        });

                        col.Item().PaddingTop(4);

                        // ── DATOS + MONTOS (2 columnas) ────────────
                        col.Item().Row(row =>
                        {
                            // Columna izquierda: datos
                            row.RelativeItem(3).Column(left =>
                            {
                                left.Spacing(1);
                                left.Item().Text($"Nro Documento   : {d.Serie} {d.Numero}");
                                left.Item().Text($"Fecha           : {d.FechaEmision}");
                                left.Item().Text($"Documento       : {d.TipoDocumento} ({d.TipoSunat})");
                                if (!string.IsNullOrWhiteSpace(d.DescTipoDocumento))
                                    left.Item().PaddingLeft(18).Text(d.DescTipoDocumento);
                                left.Item().Text($"Moneda          : {d.Moneda}");
                                left.Item().Text($"Tasa de cambio  : S/.{d.TasaCambio:F4}");
                                left.Item().Text($"Fecha recepción : {d.FechaRecepcion}");
                                left.Item().Text($"Lugar de Pago   : {d.DescLugarPago}");
                                left.Item().Text($"Reembolsar a RUC: {d.RucBenef}");
                                left.Item().Text($"Orden Compra    : {(string.IsNullOrWhiteSpace(d.OrdenCompra) ? "NINGUNO" : d.OrdenCompra)}");
                                if (d.EsDocumentoElectronico)
                                    left.Item().Text("DOCUMENTO ELECTRÓNICO");
                                if (d.EstadoSunat is "ACEPTADO" or "1")
                                    left.Item().Text("ACEPTADO POR SUNAT").Bold();
                                if (!string.IsNullOrWhiteSpace(d.FechaVencimiento))
                                    left.Item().Text($"Fec. Vencimiento : {d.FechaVencimiento}");
                            });

                            // Columna derecha: montos
                            row.RelativeItem(2).Column(right =>
                            {
                                right.Spacing(1);
                                right.Item().Row(r => { r.RelativeItem().Text("Monto neto"); r.AutoItem().AlignRight().Text(FormatMonto(d.MontoNeto)); });
                                right.Item().Row(r => { r.RelativeItem().Text("Exento"); r.AutoItem().AlignRight().Text(FormatMonto(d.MontoExento)); });
                                right.Item().PaddingTop(d.DescTipoDocumento.Length > 0 ? 9 : 0);  // alinear con la descripción
                                right.Item().Row(r => { r.RelativeItem().Text("Subtotal"); r.AutoItem().AlignRight().Text(FormatMonto(d.MontoNeto + d.MontoExento)); });
                                right.Item().Row(r => { r.RelativeItem().Text($"IGV Créd. Fisc {d.PorcentajeIGV:F1}%"); r.AutoItem().AlignRight().Text(FormatMonto(d.MontoIGVCredito)); });
                                right.Item().Row(r => { r.RelativeItem().Text("Total"); r.AutoItem().AlignRight().Text(FormatMonto(d.MontoBruto)); });
                                right.Item().Row(r => { r.RelativeItem().Text("Retención"); r.AutoItem().AlignRight().Text(FormatMonto(d.MontoRetencion)); });
                                right.Item().Row(r => { r.RelativeItem().Text("Total a pagar"); r.AutoItem().AlignRight().Text(FormatMonto(d.MontoBruto - d.MontoRetencion)); });
                            });
                        });

                        // ── APROBACIÓN / DETRACCIÓN ────────────────
                        col.Item().PaddingTop(4).Text($"Aprobación      :");
                        if (d.TieneDetraccion)
                            col.Item().Text($"Detracción      : {d.PorcentajeDetraccion:F0}% {d.TipoDetraccion}[{d.PorcentajeDetraccion:F2}%]");

                        col.Item().PaddingTop(6);

                        // ── TABLA IMPUTACIONES ─────────────────────
                        col.Item().Row(row =>
                        {
                            row.ConstantItem(75).Text("Cuenta").Bold();
                            row.ConstantItem(75).Text("Cód. Unid. 1").Bold();
                            row.ConstantItem(55).Text("Cód. Unid. 3").Bold();
                            row.ConstantItem(55).Text("Cód. Unid. 4").Bold();
                            row.ConstantItem(60).AlignRight().Text("Monto").Bold();
                            row.RelativeItem().PaddingLeft(4).Text("Descripción").Bold();
                        });
                        col.Item().Text(SEP_IGUAL);

                        // Filas de imputación
                        decimal totalImputado = 0;
                        foreach (var imp in d.Imputaciones)
                        {
                            totalImputado += imp.Monto;
                            col.Item().Row(row =>
                            {
                                row.ConstantItem(75).Text(imp.CuentaContable);
                                row.ConstantItem(75).Text(imp.CodUnidad1Cuenta);
                                row.ConstantItem(55).Text(imp.CodUnidad3Cuenta);
                                row.ConstantItem(55).Text(imp.CodUnidad4Cuenta);
                                row.ConstantItem(60).AlignRight().Text(FormatMonto(imp.Monto));
                                row.RelativeItem().PaddingLeft(4).Text(imp.Descripcion);
                            });
                        }

                        col.Item().Text(SEP_IGUAL);

                        // Total
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().AlignRight()
                               .Text(text =>
                               {
                                   text.Span("Total S/.    ").Bold();
                                   text.Span(FormatMonto(totalImputado));
                               });
                        });

                        col.Item().Text(SEP_IGUAL);

                        // ── PIE DE PÁGINA ──────────────────────────
                        col.Item().PaddingTop(8).Row(row =>
                        {
                            row.RelativeItem().Column(left =>
                            {
                                left.Item().Text("Digitador por").Bold();
                                left.Item().Text(d.RolDigitacion);
                                left.Item().Text(d.FechaDigitacion);
                            });
                            row.RelativeItem().Column(right =>
                            {
                                right.Item().Text("Autorizado por").Bold();
                                right.Item().Text(d.RolAutorizacion);
                            });
                        });

                        col.Item().PaddingTop(6).Text(SEP_GUION);
                        col.Item().Text("Observación :");
                        col.Item().Text(SEP_GUION);
                        if (!string.IsNullOrWhiteSpace(d.Observacion))
                            col.Item().Text(d.Observacion);
                    });
                });
            }).GeneratePdf();
        }

        private static string FormatMonto(decimal valor)
            => valor.ToString("N2");
    }
}
