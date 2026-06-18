using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Reciclaje.Aplicacion.DTOs.Reciclaje;
using Reciclaje.Aplicacion.Interfaces;
using Reciclaje.Dominio.Entidades;
using Reciclaje.Dominio.Interfaces;

namespace Reciclaje.Aplicacion.Servicios
{
    public class ValeRecuperoReporteServicio : IValeRecuperoReporteServicio
    {
        private readonly IValeRecuperoDetalleRepositorio _detalleRepositorio;
        private readonly IConversionarticuloRepositorio<Conversionarticulo> _conversionRepositorio;

        public ValeRecuperoReporteServicio(
            IValeRecuperoDetalleRepositorio detalleRepositorio,
            IConversionarticuloRepositorio<Conversionarticulo> conversionRepositorio)
        {
            _detalleRepositorio = detalleRepositorio;
            _conversionRepositorio = conversionRepositorio;
        }

        // ── 1. Armar datos agrupados por NumeroSRO ───────────────────
        public async Task<ValeRecuperoReporteDto> ObtenerDatosReporte(ValeRecuperoBuscarDto filtros)
        {
            var detalles = await _detalleRepositorio.BuscarParaReporte(
                filtros.NumeroSro,
                filtros.NumeroVale,
                filtros.CodigoArticuloReciclaje,
                filtros.DescripcionArticuloReciclaje,
                filtros.FechaVale);

            // Construir una fila por cada detalle (línea por línea)
            var filas = new List<ValeRecuperoReporteFilaDto>();
            foreach (var d in detalles)
            {
                var descripcion = string.Empty;
                if (!string.IsNullOrWhiteSpace(d.ArticuloReciclaje))
                {
                    var conv = await _conversionRepositorio
                        .ObtenerPorArticuloReciclaje(d.ArticuloReciclaje);
                    descripcion = conv?.DescripcionArticuloReciclaje ?? string.Empty;
                }

                filas.Add(new ValeRecuperoReporteFilaDto
                {
                    NumeroSro = d.Srolinea?.Sro?.NumeroSro ?? string.Empty,
                    CodigoSST = d.CodigoSupervisorNoInv ?? string.Empty,
                    DescripcionSST = d.DescripcionSupervisorNoInv ?? string.Empty,
                    ArticuloNoInventariado = d.ArticuloNoInv ?? string.Empty,
                    UmNoInv = d.UmnoInv ?? string.Empty,
                    ArticuloReciclaje = d.ArticuloReciclaje ?? string.Empty,
                    DescripcionArticuloReciclaje = descripcion,
                    UnidadMedidaReciclaje = d.UMReciclaje ?? string.Empty,
                    NroVale = d.Valerecupero?.NumeroVale ?? string.Empty,
                    CantidadNoInv = d.CantidadNoInv,
                    CantidadRecibida = d.CantidadRecibida,
                    Peso = d.PesoRecibido,
                    Estado = d.Valerecupero?.Estado ?? string.Empty,
                    Ruc = d.Srolinea?.Sro?.Ruc ?? string.Empty,
                    Dept = d.Srolinea?.Dept ?? string.Empty
                });
            }

            // Agrupar por NumeroSRO ordenado
            var grupos = filas
                .GroupBy(f => f.NumeroSro)
                .OrderBy(g => g.Key)
                .Select(g => new ValeRecuperoReporteGrupoDto
                {
                    NumeroSro = g.Key,
                    Filas = g.ToList()
                })
                .ToList();

            return new ValeRecuperoReporteDto
            {
                Filtros = filtros,
                Grupos = grupos,
                FechaGeneracion = DateTime.Now
            };
        }

        // ── 2. Generar PDF en memoria ─────────────────────────────────
        public async Task<byte[]> GenerarPdf(ValeRecuperoBuscarDto filtros)
        {
            var datos = await ObtenerDatosReporte(filtros);
            return GenerarDocumento(datos);
        }

        // ── 3. Construcción del documento QuestPDF ────────────────────
        private static byte[] GenerarDocumento(ValeRecuperoReporteDto datos)
        {
            const string AzulCorp = "#1E2D5A";
            const string AzulSeccion = "#2E4080";
            const string AzulClaro = "#3B5BDB";
            const string GrisHeader = "#F1F3F9";
            const string GrisLinea = "#E8EBF4";
            const string VerdeOk = "#3B6D11";
            const string AmarilloOk = "#92400E";
            const string Blanco = "#FFFFFF";

            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Arial"));

                    // ══ ENCABEZADO (se repite en cada página) ══
                    page.Header().Column(col =>
                    {
                        col.Item().Background(AzulCorp).Padding(10).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item()
                                    .Text("VALE DE RECUPERO DE LA SST")
                                    .FontSize(14).Bold().FontColor(Blanco);
                                c.Item()
                                    .Text($"Fecha de generación: {datos.FechaGeneracion:dd/MM/yyyy HH:mm}")
                                    .FontSize(8).FontColor("#ADBCE8");
                            });
                            row.ConstantItem(230).AlignRight().Column(c =>
                            {
                                c.Item()
                                    .Text($"Total registros: {datos.TotalRegistros}  |  SROs: {datos.Grupos.Count}")
                                    .FontSize(8).Bold().FontColor(Blanco);
                            });
                        });
                        col.Item().Height(4);
                    });

                    // ══ CONTENIDO: una sección por NumeroSRO ══
                    page.Content().Column(body =>
                    {
                        foreach (var grupo in datos.Grupos)
                        {
                            // ── Banda de título del SRO ──────────────
                            body.Item()
                                .Background(AzulSeccion)
                                .PaddingVertical(6).PaddingHorizontal(10)
                                .Row(row =>
                                {
                                    row.RelativeItem()
                                        .Text($"SRO: {grupo.NumeroSro}")
                                        .FontSize(10).Bold().FontColor(Blanco);
                                    row.ConstantItem(180).AlignRight()
                                        .Text($"{grupo.Filas.Count} línea(s)")
                                        .FontSize(8).FontColor("#ADBCE8");
                                });

                            // ── Tabla del grupo ──────────────────────
                            body.Item().Table(tabla =>
                            {
                                // 10 columnas: SST | Art.NoInv | Art.Rec | Descripción |
                                //              U/M | Nro Vale | Cant.Recibida | Peso | Estado | RUC
                                tabla.ColumnsDefinition(cols =>
                                {
                                    cols.RelativeColumn(2.0f);  // Art. No Inventariado
                                    cols.RelativeColumn(1.0f);  // U/M NoInv
                                    cols.RelativeColumn(1.8f);  // Art. Reciclaje
                                    cols.RelativeColumn(3.2f);  // Descripción Art. Reciclaje
                                    cols.RelativeColumn(1.2f);  // U/M
                                    cols.RelativeColumn(2.8f);  // Nro Vale
                                    cols.RelativeColumn(1.6f);  // Cant. NoInv
                                    cols.RelativeColumn(1.6f);  // Cant. Recibida
                                    cols.RelativeColumn(1.4f);  // Peso
                                    cols.RelativeColumn(1.6f);  // Estado
                                    cols.RelativeColumn(1.4f);  // CR (Dept)
                                });

                                // Cabecera
                                IContainer CeldaH(IContainer c) =>
                                    c.Background(GrisHeader)
                                     .BorderBottom(1).BorderColor(AzulCorp)
                                     .PaddingVertical(4).PaddingHorizontal(4)
                                     .AlignCenter().AlignMiddle();

                                tabla.Header(h =>
                                {
                                    h.Cell().Element(CeldaH).Text("Art. No Inventariado").Bold().FontSize(7.5f).FontColor(AzulCorp);
                                    h.Cell().Element(CeldaH).Text("U/M NoInv").Bold().FontSize(7.5f).FontColor(AzulCorp);
                                    h.Cell().Element(CeldaH).Text("Art. Reciclaje").Bold().FontSize(7.5f).FontColor(AzulCorp);
                                    h.Cell().Element(CeldaH).Text("Descripción Art. Rec.").Bold().FontSize(7.5f).FontColor(AzulCorp);
                                    h.Cell().Element(CeldaH).Text("U/M").Bold().FontSize(7.5f).FontColor(AzulCorp);
                                    h.Cell().Element(CeldaH).Text("Nro Vale").Bold().FontSize(7.5f).FontColor(AzulCorp);
                                    h.Cell().Element(CeldaH).Text("Cant. SRO").Bold().FontSize(7.5f).FontColor(AzulCorp);
                                    h.Cell().Element(CeldaH).Text("Cant. Recibida").Bold().FontSize(7.5f).FontColor(AzulCorp);
                                    h.Cell().Element(CeldaH).Text("Peso").Bold().FontSize(7.5f).FontColor(AzulCorp);
                                    h.Cell().Element(CeldaH).Text("Estado").Bold().FontSize(7.5f).FontColor(AzulCorp);
                                    h.Cell().Element(CeldaH).Text("CR").Bold().FontSize(7.5f).FontColor(AzulCorp);
                                });

                                // Filas de datos — una por cada detalle
                                for (int i = 0; i < grupo.Filas.Count; i++)
                                {
                                    var fila = grupo.Filas[i];
                                    var fondo = i % 2 == 0 ? Blanco : GrisLinea;

                                    IContainer Celda(IContainer c) =>
                                        c.Background(fondo)
                                         .BorderBottom(1).BorderColor("#E0E4EF")
                                         .PaddingVertical(4).PaddingHorizontal(4);

                                    tabla.Cell().Element(Celda).Text(fila.ArticuloNoInventariado).FontSize(7.5f);
                                    tabla.Cell().Element(Celda).AlignCenter().Text(fila.UmNoInv).FontSize(7.5f);
                                    tabla.Cell().Element(Celda).Text(fila.ArticuloReciclaje).FontSize(7.5f);
                                    tabla.Cell().Element(Celda).Text(fila.DescripcionArticuloReciclaje).FontSize(7.5f);
                                    tabla.Cell().Element(Celda).AlignCenter().Text(fila.UnidadMedidaReciclaje).FontSize(7.5f);
                                    tabla.Cell().Element(Celda).Text(fila.NroVale).Bold().FontSize(7.5f).FontColor(AzulClaro);

                                    tabla.Cell().Element(Celda).AlignRight()
                                        .Text(fila.CantidadNoInv.HasValue
                                            ? fila.CantidadNoInv.Value.ToString("N4") : "—")
                                        .FontSize(7.5f);

                                    tabla.Cell().Element(Celda).AlignRight()
                                        .Text(fila.CantidadRecibida.HasValue
                                            ? fila.CantidadRecibida.Value.ToString("N4") : "—")
                                        .FontSize(7.5f);

                                    tabla.Cell().Element(Celda).AlignRight()
                                        .Text(fila.Peso.HasValue
                                            ? fila.Peso.Value.ToString("N4") : "—")
                                        .FontSize(7.5f);

                                    var colorEstado = fila.Estado?.ToUpper() switch
                                    {
                                        "RECEPCIONADO" => VerdeOk,
                                        "CONFIRMADO" => "#14532D",
                                        "PENDIENTE" => AmarilloOk,
                                        _ => "#444444"
                                    };
                                    tabla.Cell().Element(Celda).AlignCenter()
                                        .Text(fila.Estado).Bold().FontSize(7.5f).FontColor(colorEstado);

                                    tabla.Cell().Element(Celda).AlignCenter()
                                        .Text(fila.Dept).FontSize(7.5f);

                                }

                                // ── Subtotal del grupo SRO ───────────
                                IContainer CeldaSub(IContainer c) =>
                                    c.Background(GrisHeader)
                                     .BorderTop(1).BorderColor(AzulCorp)
                                     .PaddingVertical(4).PaddingHorizontal(4);

                                tabla.Cell().ColumnSpan(7).Element(CeldaSub)
                                    .AlignRight()
                                    .Text($"Subtotal SRO {grupo.NumeroSro}")
                                    .Bold().FontSize(7.5f).FontColor(AzulCorp);
                                tabla.Cell().Element(CeldaSub).Text(string.Empty); // Cant. SRO (sin subtotal)
                                tabla.Cell().Element(CeldaSub).AlignRight()
                                    .Text(grupo.TotalCantidadRecibida.ToString("N4"))
                                    .Bold().FontSize(7.5f).FontColor(AzulCorp);
                                tabla.Cell().Element(CeldaSub).AlignRight()
                                    .Text(grupo.TotalPeso.ToString("N4"))
                                    .Bold().FontSize(7.5f).FontColor(AzulCorp);
                                tabla.Cell().ColumnSpan(3).Element(CeldaSub).Text(string.Empty);
                            });

                            // Separador entre grupos
                            body.Item().Height(10);
                        }

                        // ── Total general (solo si hay más de un SRO) ──
                        if (datos.Grupos.Count > 1)
                        {
                            body.Item()
                                .Background(AzulCorp)
                                .PaddingVertical(6).PaddingHorizontal(10)
                                .Row(row =>
                                {
                                    row.RelativeItem()
                                        .Text("TOTAL GENERAL")
                                        .Bold().FontSize(9).FontColor(Blanco);
                                    row.ConstantItem(140).AlignRight()
                                        .Text($"Cant: {datos.TotalCantidadRecibida:N4}")
                                        .Bold().FontSize(8).FontColor(Blanco);
                                    row.ConstantItem(120).AlignRight()
                                        .Text($"Peso: {datos.TotalPeso:N4}")
                                        .Bold().FontSize(8).FontColor(Blanco);
                                });
                        }
                    });

                    // ══ PIE DE PÁGINA ══
                    page.Footer().Background(GrisHeader).Padding(5).Row(row =>
                    {
                        row.RelativeItem()
                            .Text("Sistema de Gestión de Reciclaje — SST")
                            .FontSize(7).FontColor("#666666");
                        row.ConstantItem(120).AlignRight().Text(t =>
                        {
                            t.Span("Pág. ").FontSize(7).FontColor("#666666");
                            t.CurrentPageNumber().FontSize(7).FontColor("#666666");
                            t.Span(" de ").FontSize(7).FontColor("#666666");
                            t.TotalPages().FontSize(7).FontColor("#666666");
                        });
                    });
                });
            });

            return doc.GeneratePdf();
        }
    }
}
