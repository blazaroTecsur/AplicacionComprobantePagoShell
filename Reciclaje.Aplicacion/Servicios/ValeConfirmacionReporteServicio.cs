using ClosedXML.Excel;
using Reciclaje.Aplicacion.DTOs.Reciclaje;
using Reciclaje.Aplicacion.Interfaces;
using Reciclaje.Dominio.Entidades;
using Reciclaje.Dominio.Interfaces;

namespace Reciclaje.Aplicacion.Servicios
{
    /// <summary>
    /// Genera el reporte Excel "Vale de Recupero de la SST — Confirmación"
    /// agrupado por SRO usando ClosedXML.
    ///
    /// FUENTE DE DATOS: valerecupero_detalle (misma tabla que el reporte PDF
    /// de RecepcionarVale), consultada via IValeRecuperoDetalleRepositorio.BuscarParaReporte.
    ///
    /// Requiere NuGet en Reciclaje.Aplicacion:
    ///   dotnet add package ClosedXML
    ///
    /// En Program.cs registrar:
    ///   builder.Services.AddScoped&lt;IValeConfirmacionReporteServicio,
    ///                                ValeConfirmacionReporteServicio&gt;();
    /// </summary>
    public class ValeConfirmacionReporteServicio : IValeConfirmacionReporteServicio
    {
        private readonly IValeRecuperoDetalleRepositorio _detalleRepositorio;
        private readonly IConversionarticuloRepositorio<Conversionarticulo> _conversionRepositorio;

        // ── Colores corporativos ─────────────────────────────────────
        private static readonly XLColor AzulCorp = XLColor.FromHtml("#1E2D5A");
        private static readonly XLColor AzulSeccion = XLColor.FromHtml("#2E4080");
        private static readonly XLColor GrisHeader = XLColor.FromHtml("#F1F3F9");
        private static readonly XLColor GrisLinea = XLColor.FromHtml("#E8EBF4");
        private static readonly XLColor VerdeOk = XLColor.FromHtml("#3B6D11");
        private static readonly XLColor AmarilloOk = XLColor.FromHtml("#92400E");
        private static readonly XLColor RojoEstado = XLColor.FromHtml("#A32D2D");
        private static readonly XLColor Blanco = XLColor.White;

        // ── Cabeceras de columna (en orden) ──────────────────────────
        private static readonly string[] Columnas =
        {
            "Art. No Inventariado",
            "U/M NoInv",
            "Art. Reciclaje",
            "Descripción Art. Reciclaje",
            "U/M Reciclaje",
            "Nro Vale",
            "Cant. SRO",
            "Cant. Recibida",
            "Peso",
            "Estado",
            "RUC",
            "¿Confirmado?",
            "Fecha Recepción",
            "CR"
        };

        public ValeConfirmacionReporteServicio(
            IValeRecuperoDetalleRepositorio detalleRepositorio,
            IConversionarticuloRepositorio<Conversionarticulo> conversionRepositorio)
        {
            _detalleRepositorio = detalleRepositorio;
            _conversionRepositorio = conversionRepositorio;
        }

        // ── Generar Excel en memoria ─────────────────────────────────
        public async Task<byte[]> GenerarExcel(ValeRecuperoBuscarDto filtros)
        {
            // 1. Obtener detalles desde valerecupero_detalle (igual que el PDF)
            var detalles = await _detalleRepositorio.BuscarParaReporte(
                filtros.NumeroSro,
                filtros.NumeroVale,
                filtros.CodigoArticuloReciclaje,
                filtros.DescripcionArticuloReciclaje,
                filtros.FechaVale);

            // 2. Filtrar sólo estados relevantes para la confirmación
            var detallesFiltrados = detalles
                .Where(d => d.Valerecupero?.Estado == "Recepcionado"
                         || d.Valerecupero?.Estado == "Confirmado")
                .ToList();

            // 3. Construir DTOs enriquecidos con descripción de conversión
            var dtos = new List<ValeConfirmacionDto>();
            foreach (var d in detallesFiltrados)
            {
                var descripcion = string.Empty;
                if (!string.IsNullOrWhiteSpace(d.ArticuloReciclaje))
                {
                    var conv = await _conversionRepositorio
                        .ObtenerPorArticuloReciclaje(d.ArticuloReciclaje);
                    descripcion = conv?.DescripcionArticuloReciclaje ?? string.Empty;
                }

                dtos.Add(new ValeConfirmacionDto
                {
                    NumeroSro = d.Srolinea?.Sro?.NumeroSro ?? string.Empty,
                    ArticuloNoInventariado = d.ArticuloNoInv ?? string.Empty,
                    UmNoInv = d.UmnoInv ?? string.Empty,
                    CodigoArticuloReciclaje = d.ArticuloReciclaje ?? string.Empty,
                    DescripcionArticuloReciclaje = descripcion,
                    UmReciclaje = d.UMReciclaje ?? string.Empty,
                    NumeroVale = d.Valerecupero?.NumeroVale ?? string.Empty,
                    CantidadNoInv = d.CantidadNoInv,
                    CantidadReal = d.CantidadRecibida,
                    Peso = d.PesoRecibido,
                    Estado = d.Valerecupero?.Estado ?? string.Empty,
                    Ruc = d.Srolinea?.Sro?.Ruc ?? string.Empty,
                    CheckConfirmacion = d.Valerecupero?.CheckConfirmacion ?? false,
                    FechaRecepcion = d.Valerecupero?.FechaRecepcion,
                    Dept = d.Srolinea?.Dept ?? string.Empty
                });
            }

            // 4. Agrupar por SRO
            var grupos = dtos
                .GroupBy(d => d.NumeroSro)
                .OrderBy(g => g.Key)
                .Select(g => (Sro: g.Key, Filas: g.ToList()))
                .ToList();

            // 5. Construir el workbook
            using var wb = new XLWorkbook();

            // ── Hoja principal: todos los datos agrupados ────────────
            var ws = wb.Worksheets.Add("Vale de Recupero");
            int fila = 1;

            // ── Título principal ─────────────────────────────────────
            var rangeTitulo = ws.Range(fila, 1, fila, Columnas.Length);
            rangeTitulo.Merge();
            rangeTitulo.Value = "VALE DE RECUPERO DE LA SST";
            EstilarTituloPrincipal(ws, rangeTitulo, fila);
            fila++;

            // ── Subtítulo: fecha generación + filtros ────────────────
            var rangeSub = ws.Range(fila, 1, fila, Columnas.Length);
            rangeSub.Merge();
            rangeSub.Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}   |   " +
                             FiltrosComoTexto(filtros) +
                             $"   |   Total registros: {dtos.Count}   |   SROs: {grupos.Count}";
            EstilarSubtitulo(ws, rangeSub, fila);
            fila++;
            fila++; // fila en blanco

            // ── Iterar grupos ────────────────────────────────────────
            foreach (var (sro, filasSro) in grupos)
            {
                // Encabezado del grupo (banda SRO)
                var rangeSroTit = ws.Range(fila, 1, fila, Columnas.Length);
                rangeSroTit.Merge();
                rangeSroTit.Value = $"Vale de Recupero  —  SRO: {sro}";
                EstilarBandaSro(ws, rangeSroTit, fila);
                fila++;

                // Cabecera de columnas
                for (int c = 0; c < Columnas.Length; c++)
                {
                    var cell = ws.Cell(fila, c + 1);
                    cell.Value = Columnas[c];
                    EstilarCeldaHeader(cell);
                }
                fila++;

                // Filas de datos
                int filaInicioGrupo = fila;
                for (int i = 0; i < filasSro.Count; i++)
                {
                    var dto = filasSro[i];
                    bool banda = i % 2 != 0;
                    EscribirFila(ws, fila, dto, banda);
                    fila++;
                }

                // Subtotal del grupo
                int filaFinGrupo = fila - 1;
                EscribirSubtotal(ws, fila, filaInicioGrupo, filaFinGrupo, sro, Columnas.Length);
                fila++;
                fila++; // separador
            }

            // ── Total general (solo si hay más de 1 SRO) ────────────
            if (grupos.Count > 1)
            {
                var rangeTot = ws.Range(fila, 1, fila, Columnas.Length);
                rangeTot.Merge();
                rangeTot.Value = $"TOTAL GENERAL  |  " +
                                 $"Cant. Recibida: {dtos.Sum(d => d.CantidadReal ?? 0m):N4}  |  " +
                                 $"Peso: {dtos.Sum(d => d.Peso ?? 0m):N4}";
                EstilarTotalGeneral(ws, rangeTot, fila);
                fila++;
            }

            // ── Ajustar anchos de columna ────────────────────────────
            AjustarColumnas(ws);

            // ── Congelar la primera fila ─────────────────────────────
            ws.SheetView.FreezeRows(1);

            // ── Serializar a bytes ───────────────────────────────────
            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        // ════════════════════════════════════════════════════════════
        // HELPERS DE ESCRITURA
        // ════════════════════════════════════════════════════════════

        private static void EscribirFila(IXLWorksheet ws, int fila, ValeConfirmacionDto dto, bool banda)
        {
            var fondo = banda ? GrisLinea : Blanco;

            Celda(ws, fila, 1, dto.ArticuloNoInventariado, fondo);
            Celda(ws, fila, 2, dto.UmNoInv, fondo, center: true);
            Celda(ws, fila, 3, dto.CodigoArticuloReciclaje, fondo);
            Celda(ws, fila, 4, dto.DescripcionArticuloReciclaje, fondo);
            Celda(ws, fila, 5, dto.UmReciclaje, fondo, center: true);
            Celda(ws, fila, 6, dto.NumeroVale, fondo, bold: true,
                  colorTexto: XLColor.FromHtml("#3B5BDB"));

            CeldaNumero(ws, fila, 7, dto.CantidadNoInv, fondo);   // Cant. SRO
            CeldaNumero(ws, fila, 8, dto.CantidadReal, fondo);   // Cant. Recibida
            CeldaNumero(ws, fila, 9, dto.Peso, fondo);   // Peso

            // Estado con color
            var colorEstado = dto.Estado?.ToUpper() switch
            {
                "CONFIRMADO" => VerdeOk,
                "RECEPCIONADO" => XLColor.FromHtml("#1E5C8A"),
                _ => AmarilloOk
            };
            var cEstado = ws.Cell(fila, 10);
            cEstado.Value = dto.Estado ?? string.Empty;
            cEstado.Style.Fill.BackgroundColor = fondo;
            cEstado.Style.Font.Bold = true;
            cEstado.Style.Font.FontColor = colorEstado;
            cEstado.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            AplicarBordeFino(cEstado);

            Celda(ws, fila, 11, dto.Ruc, fondo);

            // ¿Confirmado? — SÍ / NO con colores
            var cConf = ws.Cell(fila, 12);
            cConf.Value = dto.CheckConfirmacion ? "SÍ" : "NO";
            cConf.Style.Fill.BackgroundColor = fondo;
            cConf.Style.Font.Bold = true;
            cConf.Style.Font.FontColor = dto.CheckConfirmacion ? VerdeOk : RojoEstado;
            cConf.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            AplicarBordeFino(cConf);

            // Fecha Recepción
            var cFecha = ws.Cell(fila, 13);
            if (dto.FechaRecepcion.HasValue)
            {
                cFecha.Value = dto.FechaRecepcion.Value;
                cFecha.Style.NumberFormat.Format = "dd/MM/yyyy HH:mm";
            }
            else
            {
                cFecha.Value = "—";
            }
            cFecha.Style.Fill.BackgroundColor = fondo;
            cFecha.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            AplicarBordeFino(cFecha);

            // CR (Dept)
            Celda(ws, fila, 14, dto.Dept, fondo, center: true);
        }

        private static void EscribirSubtotal(IXLWorksheet ws, int fila,
            int filaInicio, int filaFin, string sro, int totalColumnas)
        {
            // Etiqueta (cols 1-6)
            var rngEtiq = ws.Range(fila, 1, fila, 6);
            rngEtiq.Merge();
            rngEtiq.Value = $"Subtotal SRO {sro}";
            rngEtiq.Style.Font.Bold = true;
            rngEtiq.Style.Font.FontColor = AzulCorp;
            rngEtiq.Style.Fill.BackgroundColor = GrisHeader;
            rngEtiq.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            rngEtiq.Style.Border.TopBorder = XLBorderStyleValues.Medium;
            rngEtiq.Style.Border.TopBorderColor = AzulCorp;

            // Cant. SRO (col 7) — sin fórmula (es referencia, no suma)
            var cNoInv = ws.Cell(fila, 7);
            cNoInv.Style.Fill.BackgroundColor = GrisHeader;
            cNoInv.Style.Border.TopBorder = XLBorderStyleValues.Medium;
            cNoInv.Style.Border.TopBorderColor = AzulCorp;

            // Fórmula Cant. Recibida (col 8 = H)
            var cCant = ws.Cell(fila, 8);
            cCant.FormulaA1 = $"=SUM(H{filaInicio}:H{filaFin})";
            cCant.Style.Font.Bold = true;
            cCant.Style.Font.FontColor = AzulCorp;
            cCant.Style.Fill.BackgroundColor = GrisHeader;
            cCant.Style.NumberFormat.Format = "#,##0.0000";
            cCant.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            cCant.Style.Border.TopBorder = XLBorderStyleValues.Medium;
            cCant.Style.Border.TopBorderColor = AzulCorp;

            // Fórmula Peso (col 9 = I)
            var cPeso = ws.Cell(fila, 9);
            cPeso.FormulaA1 = $"=SUM(I{filaInicio}:I{filaFin})";
            cPeso.Style.Font.Bold = true;
            cPeso.Style.Font.FontColor = AzulCorp;
            cPeso.Style.Fill.BackgroundColor = GrisHeader;
            cPeso.Style.NumberFormat.Format = "#,##0.0000";
            cPeso.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            cPeso.Style.Border.TopBorder = XLBorderStyleValues.Medium;
            cPeso.Style.Border.TopBorderColor = AzulCorp;

            // Celdas vacías del resto
            for (int c = 10; c <= totalColumnas; c++)
            {
                var cc = ws.Cell(fila, c);
                cc.Style.Fill.BackgroundColor = GrisHeader;
                cc.Style.Border.TopBorder = XLBorderStyleValues.Medium;
                cc.Style.Border.TopBorderColor = AzulCorp;
            }
        }

        // ════════════════════════════════════════════════════════════
        // HELPERS DE ESTILO
        // ════════════════════════════════════════════════════════════

        private static void EstilarTituloPrincipal(IXLWorksheet ws, IXLRange r, int numFila)
        {
            r.Style.Fill.BackgroundColor = AzulCorp;
            r.Style.Font.Bold = true;
            r.Style.Font.FontSize = 14;
            r.Style.Font.FontColor = Blanco;
            r.Style.Font.FontName = "Arial";
            r.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            r.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Row(numFila).Height = 28;
        }

        private static void EstilarSubtitulo(IXLWorksheet ws, IXLRange r, int numFila)
        {
            r.Style.Fill.BackgroundColor = XLColor.FromHtml("#2E4080");
            r.Style.Font.FontSize = 9;
            r.Style.Font.FontColor = XLColor.FromHtml("#ADBCE8");
            r.Style.Font.FontName = "Arial";
            r.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            r.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Row(numFila).Height = 18;
        }

        private static void EstilarBandaSro(IXLWorksheet ws, IXLRange r, int numFila)
        {
            r.Style.Fill.BackgroundColor = AzulSeccion;
            r.Style.Font.Bold = true;
            r.Style.Font.FontSize = 10;
            r.Style.Font.FontColor = Blanco;
            r.Style.Font.FontName = "Arial";
            r.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            r.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Row(numFila).Height = 20;
        }

        private static void EstilarCeldaHeader(IXLCell c)
        {
            c.Style.Fill.BackgroundColor = GrisHeader;
            c.Style.Font.Bold = true;
            c.Style.Font.FontSize = 9;
            c.Style.Font.FontColor = AzulCorp;
            c.Style.Font.FontName = "Arial";
            c.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            c.Style.Alignment.WrapText = true;
            c.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
            c.Style.Border.BottomBorderColor = AzulCorp;
            AplicarBordeFino(c);
        }

        private static void EstilarTotalGeneral(IXLWorksheet ws, IXLRange r, int numFila)
        {
            r.Style.Fill.BackgroundColor = AzulCorp;
            r.Style.Font.Bold = true;
            r.Style.Font.FontSize = 10;
            r.Style.Font.FontColor = Blanco;
            r.Style.Font.FontName = "Arial";
            r.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Row(numFila).Height = 20;
        }

        private static void Celda(IXLWorksheet ws, int fila, int col,
            string? valor, XLColor fondo,
            bool bold = false, bool center = false, XLColor? colorTexto = null)
        {
            var c = ws.Cell(fila, col);
            c.Value = valor ?? string.Empty;
            c.Style.Fill.BackgroundColor = fondo;
            c.Style.Font.Bold = bold;
            c.Style.Font.FontName = "Arial";
            c.Style.Font.FontSize = 9;
            if (colorTexto is not null) c.Style.Font.FontColor = colorTexto;
            c.Style.Alignment.Horizontal = center
                ? XLAlignmentHorizontalValues.Center
                : XLAlignmentHorizontalValues.Left;
            AplicarBordeFino(c);
        }

        private static void CeldaNumero(IXLWorksheet ws, int fila, int col,
            decimal? valor, XLColor fondo)
        {
            var c = ws.Cell(fila, col);
            if (valor.HasValue)
            {
                c.Value = (double)valor.Value;
                c.Style.NumberFormat.Format = "#,##0.0000";
            }
            else
            {
                c.Value = "—";
            }
            c.Style.Fill.BackgroundColor = fondo;
            c.Style.Font.FontName = "Arial";
            c.Style.Font.FontSize = 9;
            c.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            AplicarBordeFino(c);
        }

        private static void AplicarBordeFino(IXLCell c)
        {
            c.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            c.Style.Border.RightBorder = XLBorderStyleValues.Thin;
            c.Style.Border.TopBorder = XLBorderStyleValues.Thin;
            c.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            c.Style.Border.LeftBorderColor = XLColor.FromHtml("#D0D5E8");
            c.Style.Border.RightBorderColor = XLColor.FromHtml("#D0D5E8");
            c.Style.Border.TopBorderColor = XLColor.FromHtml("#D0D5E8");
            c.Style.Border.BottomBorderColor = XLColor.FromHtml("#D0D5E8");
        }

        private static void AjustarColumnas(IXLWorksheet ws)
        {
            double[] anchos = { 22, 10, 18, 38, 14, 26, 14, 14, 14, 16, 16, 14, 20, 14 };
            for (int i = 0; i < anchos.Length; i++)
                ws.Column(i + 1).Width = anchos[i];
        }

        private static string FiltrosComoTexto(ValeRecuperoBuscarDto f)
        {
            var partes = new List<string>();
            if (!string.IsNullOrWhiteSpace(f.NumeroSro)) partes.Add($"SRO: {f.NumeroSro}");
            if (!string.IsNullOrWhiteSpace(f.NumeroVale)) partes.Add($"Vale: {f.NumeroVale}");
            if (!string.IsNullOrWhiteSpace(f.CodigoArticuloReciclaje)) partes.Add($"Art: {f.CodigoArticuloReciclaje}");
            if (f.FechaVale.HasValue) partes.Add($"Fecha: {f.FechaVale:dd/MM/yyyy}");
            return partes.Any() ? string.Join("  |  ", partes) : "Sin filtros";
        }
    }
}
