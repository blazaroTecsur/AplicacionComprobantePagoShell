using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using ComprobantePago.Application.Services;

namespace ComprobantePago.Web.Handler
{
    public class PermisosClaimsTransformation : IClaimsTransformation
    {
        private readonly IMemoryCache _cache;
        private readonly ISeguridadService _seguridad;

        public PermisosClaimsTransformation(
            IMemoryCache cache,
            ISeguridadService seguridad)
        {
            _cache = cache;
            _seguridad = seguridad;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var identity = (ClaimsIdentity)principal.Identity!;
            if (identity.HasClaim(c => c.Type == "permission"))
                return principal;

            var codUsuario = identity.FindFirst("oid")?.Value;
            var codTenant = identity.FindFirst("tid")?.Value;
            var codApp = identity.FindFirst("app")?.Value;
            var sessionId = identity.FindFirst("session_id")?.Value;
            if (string.IsNullOrEmpty(codUsuario) ||
                string.IsNullOrEmpty(codTenant) ||
                string.IsNullOrEmpty(codApp) ||
                string.IsNullOrEmpty(sessionId))
                return principal;

            var key = $"permisos-{codApp}:{sessionId}";
            if (!_cache.TryGetValue(key, out List<string> permisos))
            {
                var permisosBD = await _seguridad.ObtenerPermisos(codTenant, codUsuario, codApp);
                permisos = permisosBD?.Select(x => x.Codigo).ToList() ?? new();
                _cache.Set(key, permisos, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromHours(1),
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2)
                });
            }

            identity.AddClaims(permisos.Select(p => new Claim("permission", p)));
            return principal;
        }
    }
}
