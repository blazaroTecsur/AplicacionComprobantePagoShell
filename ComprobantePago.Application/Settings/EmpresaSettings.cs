namespace ComprobantePago.Application.Settings
{
    public class EmpresaSettings
    {
        public const string Section = "Empresa";
        public string Nombre { get; set; } = string.Empty;
        public string Ruc    { get; set; } = string.Empty;
    }
}
