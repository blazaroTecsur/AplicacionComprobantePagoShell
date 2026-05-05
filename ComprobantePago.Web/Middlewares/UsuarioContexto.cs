using ComprobantePago.Application.Interfaces;
using System.Security.Claims;

namespace ComprobantePago.Web.Middlewares
{
    /// <summary>
    /// Extrae los datos del usuario desde los claims del token OIDC de Azure Entra ID.
    /// En entornos sin token (desarrollo), usa el fallback configurado en UsuarioDesarrollo.
    /// </summary>
    public sealed class UsuarioContexto : IUsuarioContexto
    {
        public string                CodTenant  { get; }
        public string                CodUsuario { get; }
        public string                Correo     { get; }
        public string                Titulo     { get; }
        public IReadOnlyList<string> Roles      { get; }

        public UsuarioContexto(IHttpContextAccessor contexto, IConfiguration config)
        {
            var usuario = contexto.HttpContext?.User;

            string Dev(string clave) =>
                config[$"UsuarioDesarrollo:{clave}"] ?? string.Empty;

            string ObtenerClaim(params string[] tipos) => tipos
                .Select(t => usuario?.FindFirst(t)?.Value)
                .FirstOrDefault(v => !string.IsNullOrWhiteSpace(v)) ?? string.Empty;

            CodTenant  = ObtenerClaim(
                "tid",
                "http://schemas.microsoft.com/identity/claims/tenantid")
                .IfEmpty(Dev("CodTenant"));

            CodUsuario = ObtenerClaim(
                "oid",
                "http://schemas.microsoft.com/identity/claims/objectidentifier")
                .IfEmpty(Dev("CodUsuario"));

            Correo = ObtenerClaim(
                "preferred_username", "upn", "email",
                ClaimTypes.Email, ClaimTypes.Upn)
                .IfEmpty(usuario?.Identity?.Name ?? string.Empty)
                .IfEmpty(Dev("Correo"));

            Titulo = ObtenerClaim("name", ClaimTypes.Name)
                .IfEmpty(Dev("Titulo"));

            // Lee los App Roles del claim "roles" (array en el JWT de Entra ID).
            // En desarrollo (sin token) queda vacío; la política de autorización
            // local ya permite todo cuando ValidTenants está vacío.
            Roles = usuario?.FindAll("roles")
                        .Select(c => c.Value)
                        .ToList()
                        .AsReadOnly()
                    ?? (IReadOnlyList<string>)[];
        }
    }

    file static class StringExtensions
    {
        public static string IfEmpty(this string value, string fallback)
            => string.IsNullOrWhiteSpace(value) ? fallback : value;
    }
}
