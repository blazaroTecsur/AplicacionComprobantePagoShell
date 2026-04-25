using Microsoft.EntityFrameworkCore;
using Resguardo.Domain.Entities;
using Resguardo.Domain.Interfaces;
using Resguardo.Infrastructure.Data;

namespace Resguardo.Infrastructure.Repositorios
{
    public class GenericoRepositorio(DBContexto contexto) : RepositorioBase<Generico>(contexto), IGenericoRepositorio
    {
        public async Task<Generico?> Obtener(string tipo, string codigo)
        {
            var generico = await _entidades
                .Where(g => g.Tipo == tipo && g.Codigo == codigo)
                .Select(g => new Generico
                {
                    Id = g.Id,
                    Codigo = g.Codigo,
                    Descripcion = g.Descripcion
                }).FirstOrDefaultAsync();

            return generico;
        }
        public async Task<IEnumerable<Generico?>> Obtener(string tipo, string[] codigos)
        {
            var generico = await _entidades
                .Where(g => g.Tipo == tipo && codigos.Contains(g.Codigo) == true)
                .Select(g => new Generico
                {
                    Id = g.Id,
                    Codigo = g.Codigo,
                    Descripcion = g.Descripcion
                }).ToListAsync();

            return generico;
        }
    }
}
