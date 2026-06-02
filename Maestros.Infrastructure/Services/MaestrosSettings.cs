namespace Maestros.Infrastructure.Services
{
    public class MaestrosSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public MaestrosEndpoints Endpoints { get; set; } = new();

        public class MaestrosEndpoints
        {
            public string Proveedores      { get; set; } = "api/v1/proveedores";
            public string Empleados        { get; set; } = "api/v1/empleados";
            public string CodigosUnidad    { get; set; } = "api/v1/unidades";
            public string CuentasContables { get; set; } = "api/v1/cuentas-contables";
        }
    }
}
