using Microsoft.EntityFrameworkCore;
using Resguardo.Application.Common;
using Resguardo.Application.Interfaces;
using Resguardo.Application.Queries.ListarDepartamento;
using Resguardo.Application.Queries.ListarLimites;
using Resguardo.Application.Queries.ObtenerConfig;
using Resguardo.Infrastructure.Data;

namespace Resguardo.Infrastructure.QueryServices
{
    public class LimiteQueryService : ILimiteQueryService
    {
        private readonly DBContexto _contexto;
        public LimiteQueryService(DBContexto contexto)
        {
            _contexto = contexto;
        }
        public async Task<IEnumerable<ListarLimitesResponse>> Listar(ListarLimitesQuery filtro, IEnumerable<ListarDptoResponse> dptos)
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

                    return new ListarLimitesResponse
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
        public async Task<IEnumerable<ObtenerLimitesResponse>> Obtener(ObtenerLimitesQuery filtro)
        {            
            string[] estados = { Constantes.COD_ESTADO_APROBJEFE, Constantes.COD_ESTADO_APROBCLIENT };

            var tpoServicioQuery = await _contexto.Generico
                .Where(x => x.Tipo == Constantes.TIP_SERVICIO)
                .ToListAsync();
            var configQuery = await _contexto.Config
                .Where(x => x.Fecha == filtro.Fecha && x.CodDpto == filtro.CodDpto)
                .ToListAsync();
            var servQuery = await _contexto.Servicio
                .Where(x => x.Fecha == filtro.Fecha &&
                            x.SolicitudNav.CodDpto == filtro.CodDpto &&
                            estados.Contains(x.SolicitudNav.EstadoNav.Codigo))
                .ToListAsync();
            var servLookup = servQuery
                .ToLookup(c => (c.IdTpoServicio));

            var config = tpoServicioQuery
                .Select(t =>
                {
                    var spConfig = configQuery
                            .Where(c => c.IdTpoServicio == t.Id)
                            .Select(sc => new { sc.Diurno, sc.Nocturno })
                            .FirstOrDefault();
                    var spDiurno = servLookup[(t.Id)]
                            .Where(sc => sc.Turno == Constantes.TURNO_DIURNO)
                            .Sum(sc => sc.Cantidad);
                    var spNocturno = servLookup[(t.Id)]
                            .Where(sc => sc.Turno == Constantes.TURNO_NOCTURNO)
                            .Sum(sc => sc.Cantidad);

                    return new ObtenerLimitesResponse
                    {
                        TpoServicio = t.Descripcion,
                        DiurnoConfig = spConfig?.Diurno ?? 0,
                        NocturnoConfig = spConfig?.Nocturno ?? 0,
                        DiurnoSolicitado = spDiurno,
                        NocturnoSolicitado = spNocturno,
                    };
                });

            return config;
        }
    }
}