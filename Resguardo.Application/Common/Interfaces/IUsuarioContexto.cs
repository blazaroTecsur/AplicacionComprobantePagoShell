namespace Resguardo.Application.Common.Interfaces
{
    public interface IUsuarioContexto
    {
        public string CodTenant { get; }
        public string CodUsuario { get; }
        public string Correo { get; }
        public string Titulo { get; }
    }
}