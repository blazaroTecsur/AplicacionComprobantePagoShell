namespace Seguridad.Infrastructure.Services
{
    public class ApplicationSetting
    {
        public string Code { get; set; } = null!;
        public TenantSetting Corporate { get; set; } = null!;
        public TenantSetting External { get; set; } = null!;
    }
    public class TenantSetting
    {
        public string ClientId { get; set; } = null!;
        public string ClientSecret { get; set; } = null!;
        public string Authority { get; set; } = null!;
    }
    public class SecuritySetting
    {
        public string BaseUrl { get; set; } = null!;
        public string ScopeCorporate { get; set; } = null!;
        public string ScopeExternal { get; set; } = null!;
        public EndPointSetting Endpoints { get; set; } = null!;
    }
    public class EndPointSetting
    {
        public string ObtenerPermisos { get; set; } = null!;
    }
}