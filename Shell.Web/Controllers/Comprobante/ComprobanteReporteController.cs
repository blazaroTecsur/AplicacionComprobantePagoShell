using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shell.Web.Filters;

namespace Shell.Web.Controllers
{
    [Authorize]
    [Route("Comprobante")]
    public class ComprobanteReporteController : Controller
    {
        [Permission("COMP.RPT")]
        [HttpGet("Reporte/ExportarDistribucion/{id}")]
        public IActionResult ExportarDistribucion(string id) => NotFound();

        [Permission("COMP.RPT")]
        [HttpGet("Reporte/ExportarCabecera/{id}")]
        public IActionResult ExportarCabecera(string id) => NotFound();
    }
}
