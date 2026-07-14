namespace Shell.Web.Settings
{
    public class AzureSettings
    {
        public TenantSettings Corporate { get; set; }
        public TenantSettings External { get; set; }
    }
    public class TenantSettings
    {
        public string Authority { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string CallbackPath { get; set; }
    }
    public class ApiSettings
    {
        public SeguridadSettings Seguridad { get; set; }
    }
    public class SeguridadSettings
    {
        public string ScopeCorporate { get; set; }
        public string ScopeExternal { get; set; }
        public string BaseUrl { get; set; }
        public EndpointSettings Endpoints { get; set; }
    }
    public class EndpointSettings
    {
        public string Autenticar { get; set; }
        public string Actualizar { get; set; }
    }
}
