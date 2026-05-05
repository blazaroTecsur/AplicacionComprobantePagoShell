using Resguardo.Application.Common.Interfaces;

namespace Resguardo.Web.Middlewares
{
    public sealed class UsuarioContexto : IUsuarioContexto
    {
        private readonly IHttpContextAccessor _contexto;
        public string CodTenant { get; }
        public string CodUsuario { get; }
        public string Correo { get; }
        public string Titulo { get; }
        public IReadOnlyCollection<string> Permisos { get; }
        public bool TienePermiso(string permiso) => Permisos.Contains(permiso);
        public UsuarioContexto(IHttpContextAccessor contexto)
        {
            _contexto = contexto;

            var usuario = _contexto.HttpContext?.User;
            if (usuario != null)
            {
                CodUsuario = usuario.FindFirst("oid")?.Value ?? "";
                CodTenant = usuario.FindFirst("tid")?.Value ?? "";
                Correo = usuario.FindFirst("email")?.Value ?? "";
                Titulo = usuario.FindFirst("name")?.Value ?? "";
                Permisos = usuario?
                .FindAll("permission")
                .Select(c => c.Value)
                .ToList() ?? new List<string>();
            }
        }
    }
}