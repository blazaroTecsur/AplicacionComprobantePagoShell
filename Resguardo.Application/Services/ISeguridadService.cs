using Resguardo.Application.DTOs.Response;

namespace Resguardo.Application.Services
{
    public interface ISeguridadService
    {
        Task<IEnumerable<SeguridadRolResponse>> ObtenerPermisos(
            string codTenant, string codUsuario, string codApp);
    }
}