using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shell.Web.Filters;

namespace Shell.Web.Controllers
{
    [Authorize]
    [Route("Comprobante")]
    public class ComprobanteGestionarController : Controller
    {
        private void SetBaseUrl() => ViewBag.BaseUrl = "/comprobante";

        [Permission("COMP.GUARDAR")]
        [HttpGet("Nuevo")]
        public IActionResult Nuevo()
        {
            SetBaseUrl();
            return View();
        }

        [Permission("COMP.GUARDAR")]
        [HttpGet("Editar/{id}")]
        public IActionResult Editar(string id)
        {
            SetBaseUrl();
            return View();
        }

        [Permission("COMP.GUARDAR")]
        [HttpPost("Guardar")]
        public IActionResult Guardar() => NotFound();

        [Permission("COMP.GUARDAR")]
        [HttpPost("SubirDocumentos")]
        public IActionResult SubirDocumentos() => NotFound();

        [Permission("COMP.GUARDAR")]
        [HttpDelete("EliminarDocumento/{id}")]
        public IActionResult EliminarDocumento(string id) => NotFound();

        [Permission("COMP.GUARDAR")]
        [HttpPost("Imputaciones/Agregar")]
        public IActionResult AgregarImputacion() => NotFound();

        [Permission("COMP.GUARDAR")]
        [HttpPut("Imputaciones/Editar/{id}")]
        public IActionResult EditarImputacion(string id) => NotFound();

        [Permission("COMP.GUARDAR")]
        [HttpDelete("Imputaciones/Eliminar/{id}")]
        public IActionResult EliminarImputacion(string id) => NotFound();

        [Permission("COMP.ENVIAR")]
        [HttpPost("Enviar/{id}")]
        public IActionResult Enviar(string id) => NotFound();

        [Permission("COMP.ANULAR")]
        [HttpPost("Anular/{id}")]
        public IActionResult Anular(string id) => NotFound();

        [Permission("COMP.GUARDAR")]
        [HttpGet("ValidarSunat/{id}")]
        public IActionResult ValidarSunat(string id) => NotFound();
    }
}
