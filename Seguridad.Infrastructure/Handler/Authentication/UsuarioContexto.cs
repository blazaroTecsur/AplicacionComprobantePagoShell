using Microsoft.AspNetCore.Http;
using Seguridad.Abstractions.Interfaces;

namespace Seguridad.Infrastructure.Handler.Authentication
{
    public sealed class UsuarioContexto : IUsuarioContexto
    {
        private readonly IHttpContextAccessor _contexto;
        public string CodTenant { get; }
        public string CodUsuario { get; }
        public string Correo { get; }
        public string Titulo { get; }
        public IReadOnlyCollection<string> Permisos { get; }
        public string Puesto { get; }
        public string Departamento { get; }
        public string Empresa { get; }
        public string Dni { get; }
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
                Empresa = usuario.FindFirst("empresa")?.Value ?? "";
                Permisos = usuario?
                .FindAll("permission")
                .Select(c => c.Value)
                .ToList() ?? new List<string>();
            }
        }
    }
}