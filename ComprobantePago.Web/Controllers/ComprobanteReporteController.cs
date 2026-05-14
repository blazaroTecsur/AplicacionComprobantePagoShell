using Seguridad.Infrastructure.Handler.Authorization;
using ComprobantePago.Application.DTOs.Comprobante.Requests;
using ComprobantePago.Application.Interfaces.QueryServices;
using ComprobantePago.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComprobantePago.Web.Controllers
{
    [Authorize]
    [Route("Comprobante")]
    public class ComprobanteReporteController : Controller
    {
        private readonly ISytelineQueryService   _sytelineService;
        private readonly ISytelineEnvioService   _sytelineEnvio;
        private readonly IExcelSytelineService   _excelService;
        private readonly IComprobanteQueryService _queryService;
        private readonly ILogger<ComprobanteReporteController> _logger;

        public ComprobanteReporteController(
            ISytelineQueryService    sytelineService,
            ISytelineEnvioService    sytelineEnvio,
            IExcelSytelineService    excelService,
            IComprobanteQueryService queryService,
            ILogger<ComprobanteReporteController> logger)
        {
            _sytelineService = sytelineService;
            _sytelineEnvio   = sytelineEnvio;
            _excelService    = excelService;
            _queryService    = queryService;
            _logger          = logger;
        }

        [HttpPost("[action]")]
        [Permission("COMP.RPT")]
        public async Task<IActionResult> ExportarTodo([FromForm] BuscarComprobanteDto filtros)
        {
            var datos = (await _queryService.BuscarAsync(filtros)).ToList();

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var ws = workbook.Worksheets.Add("Comprobantes");

            var headers = new[] { "Folio", "Tipo", "Serie", "Número", "Proveedor",
                                   "Fecha", "Moneda", "Monto Total", "Estado", "Voucher" };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(1, i + 1).Value = headers[i];
                ws.Cell(1, i + 1).Style.Font.Bold = true;
            }

            int row = 2;
            foreach (var d in datos)
            {
                ws.Cell(row, 1).Value  = d.Folio;
                ws.Cell(row, 2).Value  = d.TipoComprobante;
                ws.Cell(row, 3).Value  = d.Serie;
                ws.Cell(row, 4).Value  = d.Numero;
                ws.Cell(row, 5).Value  = d.Proveedor;
                ws.Cell(row, 6).Value  = d.Fecha;
                ws.Cell(row, 7).Value  = d.Moneda;
                ws.Cell(row, 8).Value  = (double)d.MontoTotal;
                ws.Cell(row, 9).Value  = d.Estado;
                ws.Cell(row, 10).Value = d.VoucherSyteline?.ToString() ?? "";
                row++;
            }
            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            return File(ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Comprobantes_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }

        [HttpPost("[action]")]
        [Permission("COMP.RPT")]
        public async Task<IActionResult> ExportarDistribucionSyteline(
            [FromForm] List<string> folios,
            CancellationToken ct)
        {
            var cabeceras   = (await _sytelineService.ObtenerCabecerasSytelineAsync(folios)).ToList();
            var voucherBase = await _sytelineEnvio.ObtenerSiguienteVoucherAsync(ct);
            var voucherMap  = cabeceras
                .Select((c, i) => (c.Comprobante, Voucher: voucherBase + i))
                .ToDictionary(x => x.Comprobante, x => x.Voucher);

            var datos = (await _sytelineService.ObtenerDistribucionSytelineAsync(folios)).ToList();
            foreach (var d in datos)
                if (voucherMap.TryGetValue(d.Comprobante, out var v))
                    d.VoucherSyteline = v;

            var excel = _excelService.GenerarDistribucion(datos);
            return File(excel,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Distribucion_Syteline_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }

        [HttpPost("[action]")]
        [Permission("COMP.RPT")]
        public async Task<IActionResult> ExportarCabeceraSyteline(
            [FromForm] List<string> folios,
            CancellationToken ct)
        {
            var datos       = (await _sytelineService.ObtenerCabecerasSytelineAsync(folios)).ToList();
            var voucherBase = await _sytelineEnvio.ObtenerSiguienteVoucherAsync(ct);

            for (int i = 0; i < datos.Count; i++)
                datos[i].VoucherSyteline = voucherBase + i;

            var excel = _excelService.GenerarCabecera(datos);
            return File(excel,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Cabecera_Syteline_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }
    }
}
