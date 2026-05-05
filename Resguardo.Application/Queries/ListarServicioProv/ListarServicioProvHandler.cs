using Resguardo.Application.Interfaces;
using Resguardo.Application.Queries.ListarServicio;

namespace Resguardo.Application.Queries.ListarServicioProv
{
    public class ListarServicioProvHandler
    {
        private readonly IServicioProvQueryService _query;

        public ListarServicioProvHandler(IServicioProvQueryService query)
        {
            _query = query;
        }
        public async Task<IEnumerable<ListarServicioProvItem>> Ejecutar(int idSolicitud)
        {
            return await _query.Listar(idSolicitud);
        }
    }
}