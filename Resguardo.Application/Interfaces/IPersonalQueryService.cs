using Resguardo.Application.Queries.ObtenerPersonal;

namespace Resguardo.Application.Interfaces
{
    public interface IPersonalQueryService
    {
        public Task<ObtenerPersonalResponse> Obtener(string dni);
    }
}
