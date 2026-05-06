namespace Seguridad.Abstractions.Interfaces
{
    public interface IUsuarioContexto
    {
        public string CodTenant { get; }
        public string CodUsuario { get; }
        public string Correo { get; }
        public string Titulo { get; }
        public string Puesto { get; }
        public string Departamento { get; }
        public string Empresa { get; }
        public string Dni { get; }
    }
}
