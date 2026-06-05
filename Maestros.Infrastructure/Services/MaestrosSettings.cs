namespace Maestros.Infrastructure.Services
{
    public class MaestrosSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public MaestrosEndpoints Endpoints { get; set; } = new();
        public MaestrosAuthSettings Auth { get; set; } = new();

        public class MaestrosEndpoints
        {
            public string Proveedores      { get; set; } = null!;
            public string Empleados        { get; set; } = null!;
            public string CodigosUnidad    { get; set; } = null!;
            public string CuentasContables { get; set; } = null!;
        }

        public class MaestrosAuthSettings
        {
            public string ClientId     { get; set; } = string.Empty;
            public string ClientSecret { get; set; } = string.Empty;
            public string Authority    { get; set; } = string.Empty;
            public string Scope        { get; set; } = string.Empty;
        }
    }
}
