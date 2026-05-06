namespace Seguridad.Infrastructure.Services
{
    public class SeguridadSetting
    {
        public string TenantId { get; set; } = null!;
        public string ClientId { get; set; } = null!;
        public string ClientSecret { get; set; } = null!;
        public string Scope { get; set; } = null!;
        public string ObtenerPermisos { get; set; } = null!;
    }
}