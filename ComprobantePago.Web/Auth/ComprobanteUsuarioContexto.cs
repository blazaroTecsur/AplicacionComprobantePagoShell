using Microsoft.AspNetCore.Http;
using Seguridad.Abstractions.Interfaces;

namespace ComprobantePago.Web.Auth
{
    // Lee claims directamente del HttpContext (igual que UsuarioContexto) y sobreescribe
    // Empresa desde appsettings[TenantEmpresas], sin depender del tipo concreto de Seguridad.
    public sealed class ComprobanteUsuarioContexto : IUsuarioContexto
    {
        public string CodTenant    { get; }
        public string CodUsuario   { get; }
        public string Correo       { get; }
        public string Titulo       { get; }
        public string Puesto       { get; }
        public string Departamento { get; }
        public string Dni          { get; }
        public string Empresa      { get; }

        public ComprobanteUsuarioContexto(
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            var user = httpContextAccessor.HttpContext?.User;

            CodUsuario   = user?.FindFirst("oid")?.Value   ?? "";
            CodTenant    = user?.FindFirst("tid")?.Value   ?? "";
            Correo       = user?.FindFirst("email")?.Value ?? "";
            Titulo       = user?.FindFirst("name")?.Value  ?? "";
            Puesto       = user?.FindFirst("puesto")?.Value       ?? "";
            Departamento = user?.FindFirst("departamento")?.Value ?? "";
            Dni          = user?.FindFirst("dni")?.Value          ?? "";

            var tenantId      = CodTenant;
            var empresaConfig = configuration.GetSection("TenantEmpresas")[tenantId];
            var empresaClaim  = user?.FindFirst("empresa")?.Value ?? "";

            Empresa = !string.IsNullOrEmpty(empresaConfig) ? empresaConfig : empresaClaim;
        }
    }
}
