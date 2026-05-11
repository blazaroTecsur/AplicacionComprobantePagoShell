using ComprobantePago.Application.Interfaces;
using System.Security.Claims;

namespace ComprobantePago.Web.Middlewares
{
    public sealed class UsuarioContexto : IUsuarioContexto
    {
        public string CodTenant { get; }
        public string CodUsuario { get; }
        public string Correo { get; }
        public string Titulo { get; }
        public IReadOnlyCollection<string> Permisos { get; }
        public bool TienePermiso(string permiso) => Permisos.Contains(permiso);

        public UsuarioContexto(IHttpContextAccessor contexto)
        {
            var usuario = contexto.HttpContext?.User;
            CodUsuario = usuario?.FindFirst("oid")?.Value ?? "";
            CodTenant = usuario?.FindFirst("tid")?.Value ?? "";
            Correo = usuario?.FindFirst("email")?.Value ?? "";
            Titulo = usuario?.FindFirst("name")?.Value ?? "";
            Permisos = usuario?.FindAll("permission").Select(c => c.Value).ToList()
                         ?? (IReadOnlyCollection<string>)Array.Empty<string>();
        }
    }
}
