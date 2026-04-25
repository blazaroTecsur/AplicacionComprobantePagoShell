using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Resguardo.Web.Models;

namespace Resguardo.Web.Controllers
{
    [AllowAnonymous]
    public class ErrorController : Controller
    {
        public IActionResult Error(int code = 500, string? traceId = null)
        {
            var model = new ErrorViewModel
            {
                StatusCode = code,
                TraceId = traceId ?? HttpContext.TraceIdentifier,
                UserMessage = code switch
                {
                    400 => "La solicitud no es válida.",
                    401 => "No tienes permiso para acceder aquí.",
                    403 => "Acceso denegado.",
                    404 => "La página que buscas no existe.",
                    _ => "Ocurrió un error inesperado."
                }
            };

            return View(model);
        }
    }
}
