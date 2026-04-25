using Microsoft.EntityFrameworkCore;
using Resguardo.Application.Common;
using Resguardo.Application.Interfaces;
using Resguardo.Application.Queries.ListarConfig;
using Resguardo.Application.Queries.ListarDepartamento;
using Resguardo.Infrastructure.Data;

namespace Resguardo.Infrastructure.QueryServices
{
    public class ConfigQueryService : IConfigQueryService
    {
        private readonly DBContexto _contexto;
        public ConfigQueryService(DBContexto contexto)
        {
            _contexto = contexto;
        }
        public async Task<IEnumerable<ListarConfigResponse>> Listar(ListarConfigQuery filtro, IEnumerable<ListarDptoResponse> dptos)
        {
            var codDptoFiltro = string.IsNullOrEmpty(filtro.CodDpto) ? null : filtro.CodDpto;

            var tpoServicioQuery = await _contexto.Generico
                .Where(x => x.Tipo == Constantes.TIP_SERVICIO)
                .ToListAsync();
            var configQuery = await _contexto.Config
                .Where(x => x.Fecha == filtro.Fecha && (codDptoFiltro == null || x.CodDpto == codDptoFiltro))
                .ToListAsync();
            var dptosQuery = dptos
                .Where(x => codDptoFiltro == null || x.Codigo == codDptoFiltro);
            var configLookup = configQuery
                .ToLookup(c => (c.IdTpoServicio, c.CodDpto));

            var servicios = tpoServicioQuery
                .SelectMany(t => dptosQuery, (t, d) =>
                {
                    var spTurno = configLookup[(t.Id, d.Codigo)]
                            .Select(sc => new { sc.Id, sc.Diurno, sc.Nocturno })
                            .FirstOrDefault();

                    return new ListarConfigResponse
                    {
                        Id = spTurno?.Id ?? 0,
                        CodDpto = d.Codigo,
                        NomDpto = d.Nombre,
                        IdTpoServicio = t.Id,
                        TpoServicio = t.Descripcion,
                        Diurno = spTurno?.Diurno ?? 0,
                        Nocturno = spTurno?.Nocturno ?? 0
                    };
                })
                .OrderBy(x => x.NomDpto)
                .ThenBy(x => x.IdTpoServicio);

            return servicios;
        }
    }
}