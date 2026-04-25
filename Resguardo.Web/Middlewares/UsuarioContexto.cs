using Resguardo.Application.Commands;
using System.Security.Claims;

namespace Resguardo.Web.Middlewares
{
    public sealed class UsuarioContexto : IUsuarioContexto
    {
        public string CodTenant { get; }
        public string CodUsuario { get; }
        public string Correo { get; }
        public string Titulo { get; }
        public UsuarioContexto(IHttpContextAccessor contexto)
        {
            var usuario = contexto.HttpContext!.User;

            string ObtenerClaim(params string[] tipo) => tipo
                    .Select(t => usuario.FindFirst(t)?.Value)
                    .FirstOrDefault(v => !string.IsNullOrWhiteSpace(v)) ?? string.Empty;

            CodTenant = ObtenerClaim("tid", "http://schemas.microsoft.com/identity/claims/tenantid");
            CodUsuario = ObtenerClaim("oid", "http://schemas.microsoft.com/identity/claims/objectidentifier");
            Correo = usuario.Identity.Name; //ObtenerClaim("preferred_username", "email", "upn", ClaimTypes.Email);
            Titulo = ObtenerClaim("name", ClaimTypes.Name);
        }
    }
}