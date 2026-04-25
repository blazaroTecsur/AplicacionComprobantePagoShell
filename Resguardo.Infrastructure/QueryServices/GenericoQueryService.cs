using Microsoft.EntityFrameworkCore;
using Resguardo.Application.Interfaces;
using Resguardo.Application.Queries.ListarGenerico;
using Resguardo.Infrastructure.Data;

namespace Resguardo.Infrastructure.QueryServices
{
    public class GenericoQueryService: IGenericoQueryService
    {
        private readonly DBContexto _contexto;
        public GenericoQueryService(DBContexto contexto)
        {
            _contexto = contexto;
        }
        public async Task<IEnumerable<ListarGenericoResponse>> Listar(string[] tipos)
        {
            var genericos = await _contexto.Generico
                .Where(g => tipos.Contains(g.Tipo))
                .Select(g => new ListarGenericoResponse
                {
                    Id = g.Id,
                    Tipo = g.Tipo,
                    Codigo = g.Codigo,
                    Descripcion = g.Descripcion
                }).ToListAsync();

            return genericos;
        }        
    }
}
