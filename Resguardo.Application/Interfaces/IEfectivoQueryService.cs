using Resguardo.Application.Queries.ListarEfectivos;
using Resguardo.Application.Queries.ReporteEfectivo;

namespace Resguardo.Application.Interfaces
{
    public interface IEfectivoQueryService
    {
        public Task<IEnumerable<ListarEfectivoResponse>> Listar(int idServicioProv);        
    }
}
