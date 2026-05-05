using Microsoft.EntityFrameworkCore;
using Resguardo.Domain.Entities;
using Resguardo.Domain.Repositories;
using Resguardo.Infrastructure.Data;
using Resguardo.Infrastructure.Repositorios;

namespace Resguardo.Infrastructure.Repositories
{
    public class ConfigRepositorio(DBContexto contexto) : RepositorioBase<Limite>(contexto), IConfigRepositorio
    {
        public async Task<IEnumerable<Limite>> Listar(DateOnly fecha)
        {
            var configs = await _entidades
                .Where(c => c.Fecha == fecha)
                .Select(c => new Limite
                {
                    Fecha = c.Fecha,
                    CodDpto = c.CodDpto,
                    IdTpoServicio = c.IdTpoServicio,
                    Diurno = c.Diurno,
                    Nocturno = c.Nocturno
                }).ToListAsync();

            return configs;
        }
        public async Task<int> ObtenerDiurno(string codDpto, int idTpoServicio, DateOnly fecha)
        {
            var diurno = await _entidades
                .Where(s => s.CodDpto == codDpto && s.IdTpoServicio == idTpoServicio && s.Fecha == fecha)
                .SumAsync(s => s.Diurno);
            return diurno;
        }
        public async Task<int> ObtenerNocturno(string codDpto, int idTpoServicio, DateOnly fecha)
        {
            var nocturno = await _entidades
                .Where(s => s.CodDpto == codDpto && s.IdTpoServicio == idTpoServicio && s.Fecha == fecha)
                .SumAsync(s => s.Nocturno);
            return nocturno;
        }
    }
}
