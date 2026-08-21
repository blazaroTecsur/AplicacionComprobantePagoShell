using ComprobantePago.Application.DTOs.Comprobante.Requests;
using ComprobantePago.Application.DTOs.Comprobante.Response;
using ComprobantePago.Application.Interfaces.QueryServices;
using ComprobantePago.Application.Settings;
using ComprobantePago.Infrastructure.Extensions;
using ComprobantePago.Infrastructure.Persistence;
using ComprobantePago.Infrastructure.Services;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Seguridad.Abstractions.Interfaces;

namespace ComprobantePago.Infrastructure.QueryServices
{
    public class ComprobanteQueryService(
        AppDbContext contexto,
        ILogger<ComprobanteQueryService> logger,
        IConfiguration config,
        IUsuarioContexto usuario)
        : IComprobanteQueryService
    {
        private readonly AppDbContext _contexto = contexto;
        private readonly ILogger<ComprobanteQueryService> _logger = logger;
        private readonly IConfiguration _config = config;
        private readonly IUsuarioContexto _usuario = usuario;

        private EmpresaSettings ObtenerEmpresa() =>
            _config.GetSection($"{EmpresaSettings.Section}:{_usuario.CodigoEmpresa()}")
                   .Get<EmpresaSettings>() ?? new EmpresaSettings();

        // ── Buscar comprobantes ───────────────────
        public async Task<IEnumerable<ComprobanteDto>> BuscarAsync(
            BuscarComprobanteDto filtros)
        {
            _logger.LogInformation("Buscando comprobantes con filtros: {@Filtros}", filtros);
            var query = _contexto.Comprobantes
                .Where(x => x.CodigoEmpresa == _usuario.CodigoEmpresa())
                .AsQueryable();

            if (!string.IsNullOrEmpty(filtros.Tipo))
                query = query.Where(x => x.TipoDocumento == filtros.Tipo);

            if (!string.IsNullOrEmpty(filtros.Estado))
                query = query.Where(x => x.CodigoEstado == filtros.Estado);

            if (!string.IsNullOrEmpty(filtros.Proveedor))
                query = query.Where(x =>
                    x.RucReceptor.Contains(filtros.Proveedor) ||
                    x.RazonSocialReceptor.Contains(filtros.Proveedor));

            if (!string.IsNullOrEmpty(filtros.Folio))
                query = query.Where(x => x.Folio.Contains(filtros.Folio));

            return await query
                .OrderByDescending(x => x.FechaReg)
                .Select(x => new ComprobanteDto
                {
                    Folio = x.Folio,
                    TipoComprobante = x.TipoDocumento,
                    Serie = x.Serie,
                    Numero = x.Numero,
                    Proveedor = x.RazonSocialReceptor,
                    Fecha = x.FechaEmision.ToString("dd/MM/yyyy"),
                    Moneda = x.Moneda,
                    MontoTotal = x.MontoTotal,
                    Estado = x.CodigoEstado,
                    VoucherSyteline = x.VoucherSyteline,
                    Observacion = x.Observacion ?? string.Empty
                })
                .ToListAsync();
        }

        // ── Obtener detalle ───────────────────────
        public async Task<ComprobanteDetalleDto> ObtenerDetalleAsync(string folio)
        {
            var c = await _contexto.Comprobantes
                .FirstOrDefaultAsync(x => x.Folio == folio && x.CodigoEmpresa == _usuario.CodigoEmpresa());

            if (c == null) return null!;

            return new ComprobanteDetalleDto
            {
                Folio = c.Folio,
                Ruc = c.RucReceptor,
                RazonSocial = c.RazonSocialReceptor,
                TipoDocumento = c.TipoDocumento,
                TipoSunat = c.TipoSunat,
                Serie = c.Serie,
                Numero = c.Numero,
                FechaEmision = c.FechaEmision.ToString("yyyy-MM-dd"),
                FechaRecepcion   = c.FechaRecepcion?.ToString("yyyy-MM-dd")    ?? string.Empty,
                Moneda = c.Moneda,
                TasaCambio = c.TasaCambio,
                LugarPago        = c.LugarPago                                 ?? string.Empty,
                PlazoPago        = c.PlazoPago                                 ?? string.Empty,
                FechaVencimiento = c.FechaVencimiento?.ToString("yyyy-MM-dd")  ?? string.Empty,
                RucBenef         = c.RucBeneficiario                           ?? string.Empty,
                RazonSocialBenef = c.RazonSocialBenef                          ?? string.Empty,
                Observacion      = c.Observacion                               ?? string.Empty,
                OrdenCompra      = c.OrdenCompra                               ?? string.Empty,
                FactMultiple = c.FactMultiple,
                TieneDetraccion = c.TieneDetraccion,
                TipoDetraccion       = c.TipoDetraccion                        ?? string.Empty,
                PorcentajeDetraccion = c.PorcentajeDetraccion                  ?? 0,
                ConstanciaDeposito   = c.ConstanciaDeposito                    ?? string.Empty,
                FechaDeposito        = c.FechaDeposito?.ToString("yyyy-MM-dd") ?? string.Empty,
                MontoNeto = c.MontoNeto,
                MontoExento = c.MontoExento,
                MontoIGVCosto = c.MontoIGVCosto,
                PorcentajeIGV = c.PorcentajeIGV,
                MontoIGVCredito = c.MontoIGVCredito,
                MontoTotal = c.MontoTotal,
                MontoBruto = c.MontoBruto,
                MontoRetencion = c.MontoRetencion,
                MontoDetraccion = c.MontoDetraccion,
                CodigoEstado = c.CodigoEstado,
                Estado = c.CodigoEstado,
                RolDigitacion     = c.RolDigitacion                                  ?? string.Empty,
                FechaDigitacion   = c.FechaDigitacion?.ToString("dd/MM/yyyy HH:mm")  ?? string.Empty,
                RolAutorizacion   = c.RolAutorizacion                                 ?? string.Empty,
                FechaAutorizacion = c.FechaAutorizacion?.ToString("dd/MM/yyyy HH:mm") ?? string.Empty,
                RolAprobacion     = c.RolAprobacion                                   ?? string.Empty,
                FechaAprobacion   = c.FechaAprobacion?.ToString("dd/MM/yyyy HH:mm")   ?? string.Empty,
                RolAnulacion      = c.RolAnulacion                                    ?? string.Empty,
                FechaAnulacion    = c.FechaAnulacion?.ToString("dd/MM/yyyy HH:mm")    ?? string.Empty,
                Mensaje           = c.Mensaje                                          ?? string.Empty,
                EsEmpleado     = c.EsEmpleado,
                EmpleadoCodigo = c.EmpleadoCodigo ?? string.Empty,
                EmpleadoNombre = c.EmpleadoNombre ?? string.Empty,
                RequiereAduana = c.RequiereAduana,
                RequiereDetraccion = c.RequiereDetraccion,
                EsDocumentoElectronico = c.EsDocumentoElectronico ? "S" : "N"
            };
        }

        public async Task<byte[]?> ObtenerPdfAsync(string folio)
        {
            var c = await _contexto.Comprobantes
                .FirstOrDefaultAsync(x => x.Folio == folio && x.CodigoEmpresa == _usuario.CodigoEmpresa());
            if (c is null) return null;

            var imputaciones = await _contexto.ImputacionesContables
                .Where(x => x.Folio == folio)
                .OrderBy(x => x.Secuencia)
                .ToListAsync();

            if (!imputaciones.Any()) return null;

            // Descripción del tipo de documento
            var descTipo = await _contexto.TiposDocumento
                .Where(t => t.Codigo == c.TipoDocumento)
                .Select(t => t.Descripcion)
                .FirstOrDefaultAsync() ?? string.Empty;

            // Descripción del lugar de pago
            var descLugar = await _contexto.LugaresPago
                .Where(l => l.Codigo == c.LugarPago)
                .Select(l => l.Descripcion)
                .FirstOrDefaultAsync() ?? string.Empty;

            var empresa = ObtenerEmpresa();
            var data = new ComprobanteReporteData
            {
                EmpresaNombre = empresa.Nombre,
                EmpresaRuc    = empresa.Ruc,

                RucProveedor      = c.RucReceptor,
                RazonSocial       = c.RazonSocialReceptor,
                Serie             = c.Serie,
                Numero            = c.Numero,
                FechaEmision      = c.FechaEmision.ToString("dd/MM/yyyy"),
                TipoDocumento     = c.TipoDocumento,
                TipoSunat         = c.TipoSunat,
                DescTipoDocumento = descTipo,
                Moneda            = c.Moneda,
                TasaCambio        = c.TasaCambio,
                FechaRecepcion    = c.FechaRecepcion?.ToString("dd/MM/yyyy") ?? string.Empty,
                LugarPago         = c.LugarPago ?? string.Empty,
                DescLugarPago     = descLugar,
                RucBenef          = c.RucBeneficiario ?? string.Empty,
                OrdenCompra       = c.OrdenCompra ?? string.Empty,
                FechaVencimiento  = c.FechaVencimiento?.ToString("dd/MM/yyyy") ?? string.Empty,
                EsDocumentoElectronico = c.EsDocumentoElectronico,
                EstadoSunat       = c.EstadoSunat ?? string.Empty,
                CodigoEstado      = c.CodigoEstado,

                MontoNeto        = c.MontoNeto,
                MontoExento      = c.MontoExento,
                PorcentajeIGV    = c.PorcentajeIGV,
                MontoIGVCredito  = c.MontoIGVCredito,
                MontoBruto       = c.MontoBruto,
                MontoRetencion   = c.MontoRetencion,
                MontoDetraccion  = c.MontoDetraccion,

                TieneDetraccion      = c.TieneDetraccion,
                TipoDetraccion       = c.TipoDetraccion ?? string.Empty,
                PorcentajeDetraccion = c.PorcentajeDetraccion ?? 0,

                RolDigitacion   = c.RolDigitacion ?? string.Empty,
                FechaDigitacion = c.FechaDigitacion?.ToString("dd/MM/yyyy") ?? string.Empty,
                RolAutorizacion = c.RolAutorizacion ?? string.Empty,
                Observacion     = c.Observacion ?? string.Empty,

                Imputaciones = imputaciones.Select(i => new ImputacionReporteData
                {
                    CuentaContable   = i.CuentaContable ?? string.Empty,
                    CodUnidad1Cuenta = i.CodUnidad1Cuenta ?? string.Empty,
                    CodUnidad2Cuenta = i.CodUnidad2Cuenta ?? string.Empty,
                    CodUnidad3Cuenta = i.CodUnidad3Cuenta ?? string.Empty,
                    CodUnidad4Cuenta = i.CodUnidad4Cuenta ?? string.Empty,
                    Monto            = i.Monto,
                    Descripcion      = i.Descripcion ?? string.Empty
                }).ToList()
            };

            return ComprobantePdfReporteService.Generar(data);
        }

        public async Task<IEnumerable<DocumentoElectronicoDto>>
            ObtenerDocumentosElectronicosAsync(string folio)
        {
            return await _contexto.DocumentosElectronicos
                .Where(x => x.Folio == folio)
                .OrderBy(x => x.FechaReg)
                .Select(x => new DocumentoElectronicoDto
                {
                    IdDocumento = x.IdDocumento,
                    TipoArchivo = x.TipoArchivo,
                    SubTipo = x.SubTipo,
                    NombreArchivo = x.NombreArchivo,
                    FechaReg = x.FechaReg.ToString("dd/MM/yyyy HH:mm"),
                    TamanioBytes = x.Contenido.Length
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<ImputacionDetalleDto>>
            ObtenerImputacionesAsync(string folio)
        {
            _logger.LogInformation("Obteniendo imputaciones para folio {Folio}", folio);

            var lista = await _contexto.ImputacionesContables
                .Where(x => x.Folio == folio)
                .OrderBy(x => x.Secuencia)
                .ToListAsync();

            return lista.Adapt<IEnumerable<ImputacionDetalleDto>>();
        }

        public Task<byte[]> ObtenerPlantillaImputacionAsync()
        {
            using var wb = new ClosedXML.Excel.XLWorkbook();

            // ── Hoja 1: Imputaciones ──────────────────────────────────────────
            var ws = wb.Worksheets.Add("Imputaciones");

            string[] headers = [
                "Secuencia", "AliasCuenta", "CuentaContable", "DescripcionCuenta",
                "Monto", "TipoLinea", "Descripcion", "Proyecto",
                "CodUnidad1", "CodUnidad2", "CodUnidad3", "CodUnidad4"
            ];

            // Encabezados
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightBlue;
                cell.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
            }

            // Filas de ejemplo por tipo de afectación
            var ejemplos = new[]
            {
                //  Seq   Alias       CuentaContable  Descripcion             Monto      TipoLinea   Desc                    Proyecto   U1  U2  U3  U4
                (1, "CUENTA-01", "42110001", "Proveedores Nacionales",      0m,       "CABECERA", "Cabecera SyteLine",    "",        "",  "",  "",  ""),
                (2, "CUENTA-02", "60110001", "Costo de Servicios",       1000m,       "GRAVADO",  "Monto Neto",           "PROY-01", "A1", "B2", "",  ""),
                (3, "CUENTA-03", "40111001", "IGV por Pagar",             180m,       "IGV",      "IGV Crédito Fiscal",   "",        "",  "",  "",  ""),
                (4, "CUENTA-04", "63900001", "Servicios No Gravados",     500m,       "EXENTO",   "Monto Exento",         "",        "",  "",  "",  ""),
            };

            var coloresFila = new Dictionary<string, ClosedXML.Excel.XLColor>
            {
                ["CABECERA"] = ClosedXML.Excel.XLColor.LightGray,
                ["GRAVADO"]  = ClosedXML.Excel.XLColor.FromArgb(198, 224, 180), // verde claro
                ["IGV"]      = ClosedXML.Excel.XLColor.FromArgb(189, 215, 238), // azul claro
                ["EXENTO"]   = ClosedXML.Excel.XLColor.FromArgb(255, 242, 204), // amarillo claro
            };

            for (int r = 0; r < ejemplos.Length; r++)
            {
                var (seq, alias, cuenta, descCuenta, monto, tipo, desc, proy, u1, u2, u3, u4) = ejemplos[r];
                object[] valores = [seq, alias, cuenta, descCuenta, monto, tipo, desc, proy, u1, u2, u3, u4];

                var color = coloresFila.TryGetValue(tipo, out var c) ? c : ClosedXML.Excel.XLColor.NoColor;
                for (int col = 0; col < valores.Length; col++)
                {
                    var cell = ws.Cell(r + 2, col + 1);
                    cell.Value   = ClosedXML.Excel.XLCellValue.FromObject(valores[col]);
                    cell.Style.Fill.BackgroundColor = color;
                    cell.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                    cell.Style.Font.Italic = true;
                    cell.Style.Font.FontColor = ClosedXML.Excel.XLColor.Gray;
                }
            }

            // Nota instructiva debajo de los ejemplos
            var notaCell = ws.Cell(ejemplos.Length + 3, 1);
            notaCell.Value = "⚠ Elimine las filas de ejemplo antes de cargar. La fila de CABECERA (Secuencia=1) no se importa.";
            notaCell.Style.Font.Bold = true;
            notaCell.Style.Font.FontColor = ClosedXML.Excel.XLColor.DarkRed;
            ws.Range(notaCell, ws.Cell(ejemplos.Length + 3, headers.Length)).Merge();

            ws.Columns().AdjustToContents();

            // ── Hoja 2: Glosario ──────────────────────────────────────────────
            var wg = wb.Worksheets.Add("Glosario");

            // Título
            var titulo = wg.Cell(1, 1);
            titulo.Value = "Glosario de columnas — Plantilla de Imputación Contable";
            titulo.Style.Font.Bold = true;
            titulo.Style.Font.FontSize = 13;
            titulo.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
            titulo.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromArgb(91, 116, 173);
            wg.Range("A1:C1").Merge();

            // Encabezado de tabla
            string[] glosHeaders = ["Columna", "Obligatorio", "Descripción"];
            for (int i = 0; i < glosHeaders.Length; i++)
            {
                var cell = wg.Cell(2, i + 1);
                cell.Value = glosHeaders[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightBlue;
                cell.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
            }

            var glosario = new[]
            {
                ("Secuencia",       "No",  "Número de orden de la línea. Solo informativo; el sistema lo reasigna al importar."),
                ("AliasCuenta",     "Sí",  "Código alias de la cuenta contable (clave de búsqueda en el sistema)."),
                ("CuentaContable",  "Sí",  "Código completo de la cuenta contable según el plan de cuentas."),
                ("DescripcionCuenta","Sí", "Descripción de la cuenta contable."),
                ("Monto",           "Sí",  "Importe de la línea en la moneda del comprobante. Use punto (.) como separador decimal."),
                ("TipoLinea",       "Sí",  "Tipo de afectación contable. Valores válidos: GRAVADO · IGV · EXENTO · CABECERA.\n  - GRAVADO : línea de gasto (monto neto). Cada línea = distribución independiente en Syteline.\n  - IGV     : impuesto al valor agregado (una sola línea por comprobante).\n  - EXENTO  : monto no gravado / exonerado.\n  - CABECERA: línea de control de la cuenta A/P. No se importa."),
                ("Descripcion",     "No",  "Texto libre que describe la línea de imputación."),
                ("Proyecto",        "No",  "Código de proyecto asociado a la línea (máx. 10 caracteres)."),
                ("CodUnidad1",      "No",  "Código de unidad de análisis 1 (ej. centro de costo)."),
                ("CodUnidad2",      "No",  "Código de unidad de análisis 2."),
                ("CodUnidad3",      "No",  "Código de unidad de análisis 3."),
                ("CodUnidad4",      "No",  "Código de unidad de análisis 4."),
            };

            bool sombreado = false;
            for (int r = 0; r < glosario.Length; r++)
            {
                var (col, oblig, desc) = glosario[r];
                var rowIdx = r + 3;
                var color  = sombreado ? ClosedXML.Excel.XLColor.FromArgb(242, 242, 242) : ClosedXML.Excel.XLColor.White;
                sombreado  = !sombreado;

                void SetGlos(int c, object v)
                {
                    var cell = wg.Cell(rowIdx, c);
                    cell.Value = ClosedXML.Excel.XLCellValue.FromObject(v);
                    cell.Style.Fill.BackgroundColor = color;
                    cell.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                    cell.Style.Alignment.WrapText = true;
                    cell.Style.Alignment.Vertical = ClosedXML.Excel.XLAlignmentVerticalValues.Top;
                }

                SetGlos(1, col);
                var cOblig = wg.Cell(rowIdx, 2);
                cOblig.Value = oblig;
                cOblig.Style.Fill.BackgroundColor = color;
                cOblig.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                cOblig.Style.Alignment.Horizontal  = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                cOblig.Style.Font.Bold = oblig == "Sí";
                cOblig.Style.Font.FontColor = oblig == "Sí"
                    ? ClosedXML.Excel.XLColor.DarkGreen
                    : ClosedXML.Excel.XLColor.Gray;
                SetGlos(3, desc);
            }

            wg.Column(1).Width = 18;
            wg.Column(2).Width = 12;
            wg.Column(3).Width = 70;

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return Task.FromResult(ms.ToArray());
        }
    }
}
