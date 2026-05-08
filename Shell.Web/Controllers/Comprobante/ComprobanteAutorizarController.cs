using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shell.Web.Filters;

namespace Shell.Web.Controllers
{
    [Authorize]
    [Permission("COMP.AUTORIZAR")]
    [Route("Comprobante")]
    public class ComprobanteAutorizarController : Controller
    {
        private void SetBaseUrl() => ViewBag.BaseUrl = "/comprobante";

        [HttpGet("Autorizar/{id}")]
        public IActionResult Autorizar(string id)
        {
            SetBaseUrl();
            return View();
        }

        [HttpPost("Firmar/{id}")]
        public IActionResult Firmar(string id) => NotFound();

        [HttpPost("Derivar/{id}")]
        public IActionResult Derivar(string id) => NotFound();
    }
}
