
using Seguridad.Abstractions.DTOs;

namespace Seguridad.Abstractions.Interfaces
{
    public interface ISeguridadService
    {
        Task<IEnumerable<SeguridadRolResponse>> ObtenerPermisos(
             string type, string codTenant, string codUsuario, string codApp);
    }
}