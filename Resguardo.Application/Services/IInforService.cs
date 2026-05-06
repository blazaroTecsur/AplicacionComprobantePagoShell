using Resguardo.Application.DTOs.Infor;

namespace Resguardo.Application.Services
{
    public interface IInforService
    {
        Task<ObtenerOrdenResponse> ObtenerOrden(string sro);
    }
}