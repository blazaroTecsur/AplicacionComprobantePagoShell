using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Shell.Web.Helpers;

namespace Shell.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IWebHostEnvironment _env;
        public AuthController(IWebHostEnvironment env)
        {
            _env = env;
        }
        public IActionResult SelectTenant()
        {
            ViewBag.Entorno = _env.EnvironmentName;
            return View();
        }
        public IActionResult Login(string schema)
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = "/Home/Index",
                Items = { ["scheme"] = schema }
            };
            return Challenge(properties, schema);
        }
        public async Task<IActionResult> Logout()
        {
            var scheme = User.FindFirst("auth_scheme")?.Value;
            scheme ??= Constante.SCHEMA_CORPORATE;

            await HttpContext.SignOutAsync(scheme);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("SelectTenant", "Auth");
        }
    }
}