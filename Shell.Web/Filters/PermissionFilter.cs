using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Shell.Web.Services;

namespace Shell.Web.Filters
{
    public class PermissionFilter : IAsyncAuthorizationFilter
    {
        private readonly string _codigo;
        private readonly PermisosService _permisosService;

        public PermissionFilter(string codigo, PermisosService permisosService)
        {
            _codigo = codigo;
            _permisosService = permisosService;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!(user?.Identity?.IsAuthenticated ?? false))
            {
                context.Result = new RedirectToActionResult("SelectTenant", "Auth", null);
                return;
            }

            var codTenant = user.FindFirst("tenant")?.Value ?? "";
            var codUsuario = user.FindFirst("oid")?.Value ?? "";

            var permisos = await _permisosService.ObtenerAsync(codTenant, codUsuario);

            if (!permisos.Contains(_codigo, StringComparer.OrdinalIgnoreCase))
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
