using Microsoft.AspNetCore.Http;
using Seguridad.Abstractions.Interfaces;

namespace ComprobantePago.Web.Auth
{
    /// <summary>
    /// Decora IUsuarioContexto para resolver Empresa desde appsettings.json
    /// usando X-Tenant-Id, sin depender del shell ni de Seguridad.Infrastructure.
    /// </summary>
    public sealed class ComprobanteUsuarioContexto : IUsuarioContexto
    {
        private readonly IUsuarioContexto _inner;

        public string CodTenant    => _inner.CodTenant;
        public string CodUsuario   => _inner.CodUsuario;
        public string Correo       => _inner.Correo;
        public string Titulo       => _inner.Titulo;
        public string Puesto       => _inner.Puesto;
        public string Departamento => _inner.Departamento;
        public string Dni          => _inner.Dni;
        public string Empresa      { get; }

        public ComprobanteUsuarioContexto(
            IUsuarioContexto inner,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            _inner = inner;

            var tenantId = httpContextAccessor.HttpContext?
                .Request.Headers["X-Tenant-Id"].FirstOrDefault()
                ?? inner.CodTenant;

            var empresa = configuration
                .GetSection("TenantEmpresas")[tenantId];

            Empresa = !string.IsNullOrEmpty(empresa) ? empresa : inner.Empresa;
        }
    }
}
