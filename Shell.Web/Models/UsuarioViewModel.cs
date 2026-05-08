namespace Shell.Web.Models
{
    public class UsuarioViewModel
    {
        public string CodTenant { get; set; } = null!;
        public string NomTenant { get; set; } = null!;
        public string CodUsuario { get; set; } = null!;
        public string Correo { get; set; } = null!;
        public string Titulo { get; set; } = null!;
        public string? Puesto { get; set; }
        public string? Departamento { get; set; }
        public string? Empresa { get; set; }
        public string? Dni { get; set; }
        public ICollection<MenuViewModel> Menus { get; set; } = new List<MenuViewModel>();
        public IReadOnlyCollection<string> Permisos { get; set; } = [];
    }
}