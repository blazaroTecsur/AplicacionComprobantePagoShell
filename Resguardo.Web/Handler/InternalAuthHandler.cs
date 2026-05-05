using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Resguardo.Web.Handler
{
    public class InternalAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {        
        private readonly IConfiguration _config;
        public InternalAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,     
            UrlEncoder encoder,
            IConfiguration config)
            : base(options, logger, encoder)
        {            
            _config = config;
        }
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {            
            var codUsuario = Request.Headers["X-User-Oid"].FirstOrDefault();
            var codTenant = Request.Headers["X-Tenant-Id"].FirstOrDefault();
            var usuCorreo = Request.Headers["X-User-Email"].FirstOrDefault();
            var nomUsuario = Request.Headers["X-User-Name"].FirstOrDefault();
            var sessionId = Request.Headers["X-Session-Id"].FirstOrDefault();
            var codApp = _config["codApplication"];

            if (string.IsNullOrEmpty(codUsuario) ||
                string.IsNullOrEmpty(codTenant) ||
                string.IsNullOrEmpty(codApp) ||
                string.IsNullOrEmpty(sessionId) ||
                string.IsNullOrEmpty(usuCorreo))
                return AuthenticateResult.Fail("Internal Auth ha fallado.");

            var claims = new List<Claim>
            {                
                new Claim("oid", codUsuario),
                new Claim("tid", codTenant),
                new Claim("email", usuCorreo),
                new Claim("name", nomUsuario),
                new Claim("session_id", sessionId),
                new Claim("app", codApp)
            };           
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }
    }
}