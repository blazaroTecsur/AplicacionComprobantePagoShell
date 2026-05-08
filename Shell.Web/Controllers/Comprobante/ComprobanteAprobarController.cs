using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shell.Web.Filters;

namespace Shell.Web.Controllers.Comprobante
{
    [Authorize]
    [Permission("COMP.APROBAR")]
    [Route("Comprobante")]
    public class ComprobanteAprobarController : Controller
    {
        private void SetBaseUrl() => ViewBag.BaseUrl = "/comprobante";

        [HttpGet("Aprobar/{id}")]
        public IActionResult Aprobar(string id)
        {
            SetBaseUrl();
            return View();
        }

        [HttpPost("Aprobar/{id}")]
        public IActionResult AprobarConfirmar(string id) => NotFound();

        [HttpPost("EnviarASyteline/{id}")]
        public IActionResult EnviarASyteline(string id) => NotFound();
    }
}
