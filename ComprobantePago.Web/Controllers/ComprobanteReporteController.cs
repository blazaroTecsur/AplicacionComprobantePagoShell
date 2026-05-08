using ComprobantePago.Web.Authorization;
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
        private readonly ISytelineQueryService  _sytelineService;
        private readonly ISytelineEnvioService  _sytelineEnvio;
        private readonly IExcelSytelineService  _excelService;
        private readonly ILogger<ComprobanteReporteController> _logger;

        public ComprobanteReporteController(
            ISytelineQueryService sytelineService,
            ISytelineEnvioService sytelineEnvio,
            IExcelSytelineService excelService,
            ILogger<ComprobanteReporteController> logger)
        {
            _sytelineService = sytelineService;
            _sytelineEnvio   = sytelineEnvio;
            _excelService    = excelService;
            _logger          = logger;
        }

        [HttpPost]
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

        [HttpPost]
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
