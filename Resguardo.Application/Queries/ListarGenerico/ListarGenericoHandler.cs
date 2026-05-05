using Resguardo.Application.Interfaces;
using Resguardo.Application.Queries.ListarGenerico;

namespace Resguardo.Application.Queries.ConsultarSolicitud
{
    public class ListarGenericoHandler
    {
        private readonly IGenericoQueryService _query;
        public ListarGenericoHandler(IGenericoQueryService query)
        {
            _query = query;
        }
        public async Task<IEnumerable<ListarGenericoResponse>> Ejecutar(string[] tipos)
        {
            return await _query.Listar(tipos);
        }
    }
}