namespace Maestros.Infrastructure.Services
{
    public class MaestrosSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public MaestrosEndpoints Endpoints { get; set; } = new();
        public MaestrosAuthSettings Auth { get; set; } = new();

        public class MaestrosEndpoints
        {
            public string Proveedores      { get; set; } = "api/v1/proveedores";
            public string Empleados        { get; set; } = "api/v1/empleados";
            public string CodigosUnidad    { get; set; } = "api/v1/unidades";
            public string CuentasContables { get; set; } = "api/v1/cuentas-contables";
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
