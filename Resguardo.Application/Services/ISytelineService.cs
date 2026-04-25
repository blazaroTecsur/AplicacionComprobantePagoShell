using Resguardo.Application.Queries.ObtenerOrden;

namespace Resguardo.Application.Services
{
    public interface ISytelineService
    {
        Task<ObtenerOrdenResponse> ObtenerOrden(string sro);
    }
}