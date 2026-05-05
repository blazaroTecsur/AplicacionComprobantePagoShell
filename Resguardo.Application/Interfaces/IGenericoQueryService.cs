using Resguardo.Application.Queries.ListarGenerico;

namespace Resguardo.Application.Interfaces
{
    public interface IGenericoQueryService
    {
        Task<IEnumerable<ListarGenericoResponse>> Listar(string[] tipos);        
    }
}