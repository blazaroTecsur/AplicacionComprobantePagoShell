using Resguardo.Application.Queries.ObtenerOrden;

namespace Resguardo.Application.Services
{
    public interface IInforService
    {
        Task<ObtenerOrdenResponse> ObtenerOrden(string sro);
    }
}