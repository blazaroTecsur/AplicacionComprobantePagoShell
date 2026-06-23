namespace Shell.Web.Models.ObtenerUsuario
{
    public class MenuViewModel
    {
        public string Codigo { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string? Ruta { get; set; }
        public string? Icono { get; set; }
        public AppViewModel App { get; set; } = new AppViewModel();
    }
}
