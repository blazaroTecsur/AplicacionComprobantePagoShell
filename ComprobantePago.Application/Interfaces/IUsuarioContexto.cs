namespace ComprobantePago.Application.Interfaces
{
    public interface IUsuarioContexto
    {
        string CodTenant { get; }
        string CodUsuario { get; }
        string Correo { get; }
        string Titulo { get; }
        IReadOnlyCollection<string> Permisos { get; }
        bool TienePermiso(string permiso);
    }
}
