using Resguardo.Application.Interfaces;

namespace Resguardo.Application.Queries.ListarServicio
{
    public class ListarServicioHandler
    {
        private readonly IServicioQueryService _query;
        public ListarServicioHandler(IServicioQueryService query)
        {
            _query = query;
        }
        public async Task<IEnumerable<ListarServicioResponse>> Ejecutar(int idSolicitud)
        {
            return await _query.Listar(idSolicitud);
        }
    }
}