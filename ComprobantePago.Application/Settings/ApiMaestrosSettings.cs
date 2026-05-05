namespace ComprobantePago.Application.Settings
{
    public class ApiMaestrosSettings
    {
        public const string Section = "ApiMaestros";
        public string BaseUrl { get; set; } = string.Empty;
        public bool UsarApi { get; set; } = false;
        public ApiEndpoints Endpoints { get; set; } = new();

        public class ApiEndpoints
        {
            public string Empleados { get; set; } = "/empleados";
            public string Proveedores { get; set; } = "/proveedores";
            public string CodigosUnidad { get; set; } = "/codigos-unidad";
            public string CuentasContables { get; set; } = "/cuentas-contables";
        }
    }
}
