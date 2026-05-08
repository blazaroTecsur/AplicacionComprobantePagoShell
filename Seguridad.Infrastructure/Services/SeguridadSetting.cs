namespace Seguridad.Infrastructure.Services
{
    public class SeguridadSetting
    {
        public TenantSetting Corporate { get; set; } = null!;
        public TenantSetting External { get; set; } = null!;
    }
    public class TenantSetting
    {        
        public string ClientId { get; set; } = null!;
        public string ClientSecret { get; set; } = null!;
        public string Authority { get; set; } = null!;
        public string Scope { get; set; } = null!;        
        public string ObtenerPermisos { get; set; } = null!;
    }
}