using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Resguardo.Application.DTOs.Response;
using Resguardo.Application.Services;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Resguardo.Web.Handler
{
    public class InternalAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly ISeguridadService _seguridad;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _config;
        public InternalAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            ISeguridadService seguridad,
            UrlEncoder encoder,
            IConfiguration config,
            IMemoryCache cache)
            : base(options, logger, encoder)
        {
            _seguridad = seguridad;
            _cache = cache;
            _config = config;
        }
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var codUsuario = Request.Headers["X-User-Oid"].FirstOrDefault();
            var codTenant = Request.Headers["X-Tenant-Id"].FirstOrDefault();
            var sessionId = Request.Headers["X-Session-Id"].FirstOrDefault();                      
            var codApp = _config["codApplication"];

            if (string.IsNullOrEmpty(codUsuario) || string.IsNullOrEmpty(codTenant))
                return AuthenticateResult.Fail("Internal Auth ha fallado.");

            var claims = new List<Claim>
            {
                new Claim("oid", codUsuario),
                new Claim("tid", codTenant ?? ""),
                new Claim("session_id", sessionId ?? "")
            };

            var key = $"permisos:{codApp}"; 
            List<SeguridadRolResponse> permisos = [];
            if (!_cache.TryGetValue(key, out permisos))
            {
                permisos = (await _seguridad.ObtenerPermisos(codTenant, codUsuario, codApp)).ToList();
                _cache.Set(key, permisos, TimeSpan.FromHours(8));
            }
            if (permisos is not null)
                claims.AddRange(permisos.Select(r => new Claim("permission", r.Codigo)));

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }       
    }
}