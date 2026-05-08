using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shell.Web.Filters;

namespace Shell.Web.Controllers.Comprobante
{
    [Authorize]
    [Permission("COMP.VER")]
    [Route("Comprobante")]
    public class ComprobanteConsultarController : Controller
    {
        private void SetBaseUrl() => ViewBag.BaseUrl = "/comprobante";

        public IActionResult Index()
        {
            SetBaseUrl();
            return View();
        }

        [HttpGet("Detalle/{id}")]
        public IActionResult Detalle(string id)
        {
            SetBaseUrl();
            return View();
        }

        [HttpGet("Buscar")]
        public IActionResult Buscar()
        {
            SetBaseUrl();
            return View();
        }

        /* ── Catálogos (AJAX) ── */

        [HttpGet("Catalogo/TiposDocumento")]
        public IActionResult CatalogoTiposDocumento() => Proxy();

        [HttpGet("Catalogo/Proveedores")]
        public IActionResult CatalogoProveedores() => Proxy();

        [HttpGet("Catalogo/CuentasContables")]
        public IActionResult CatalogoCuentasContables() => Proxy();

        [HttpGet("Catalogo/Empleados")]
        public IActionResult CatalogoEmpleados() => Proxy();

        [HttpGet("Catalogo/UnidadesMedida")]
        public IActionResult CatalogoUnidadesMedida() => Proxy();

        /* ── Imputaciones lectura (AJAX) ── */

        [HttpGet("Imputaciones/{comprobanteId}")]
        public IActionResult ObtenerImputaciones(string comprobanteId) => Proxy();

        /* All catalogue/detail AJAX calls are routed through YARP (/comprobante/api/**),
           these actions serve only MVC view rendering. */
        private IActionResult Proxy() => NotFound();
    }
}
