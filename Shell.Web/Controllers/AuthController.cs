using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using System.Runtime;

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
        public IActionResult Login(string tenant)
        {
            //var host = HttpContext.Request.Host.Host;
            //var tenant = _settings.Tenants.FirstOrDefault(t => t.Domain.Equals(host, StringComparison.OrdinalIgnoreCase));
            //if (tenant == null)
            //    throw new Exception("Tenant no válido");

            var host = HttpContext.Request.Host.Host;
            var properties = new AuthenticationProperties
            {
                RedirectUri = "/Home/Index"
            };
            properties.Items["tenant"] = tenant;
            return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
        }        
        public async Task<IActionResult> Logout()
        {            
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
            return RedirectToAction("SelectTenant", "Auth");            
        }
    }
}