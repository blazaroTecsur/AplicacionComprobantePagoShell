using Microsoft.EntityFrameworkCore;
using Resguardo.Application.Common;
using Resguardo.Application.Interfaces;
using Resguardo.Application.Queries.ConsultarServicio;
using Resguardo.Application.Queries.ListarServicio;
using Resguardo.Domain.Entities;
using Resguardo.Infrastructure.Data;

namespace Resguardo.Infrastructure.QueryServices
{
    public class ServicioQueryService : IServicioQueryService
    {
        private readonly DBContexto _contexto;
        public ServicioQueryService(DBContexto contexto)
        {
            _contexto = contexto;
        }
        public async Task<IEnumerable<ListarServicioResponse>> Listar(int idSolicitud)
        {
            var contratistaQuery = await _contexto.Generico
                .Where(x => x.Tipo == Constantes.TIP_PROVEEDOR)
                .ToListAsync();
            var serviciosQuery = await _contexto.Servicio
                .Include(x => x.TpoServicioNav)
                .Include(x => x.ServicioProvs)
                .Where(x => x.IdSolicitud == idSolicitud)
                .ToListAsync();

            var servicios = serviciosQuery
                .Select(s => new ListarServicioResponse
                {
                    Id = s.Id,
                    IdSolicitud = s.IdSolicitud,
                    TpoServicio = s.TpoServicioNav.Descripcion,
                    Fecha = s.Fecha,
                    HraInicio = s.HraInicio,
                    HraFinal = s.HraFinal,
                    DiaSig = s.DiaSig,
                    Coordenada = s.Coordenada,
                    Direccion = s.Direccion,
                    Cantidad = s.Cantidad,
                    ServicioProvs = contratistaQuery.Select(c =>
                    {
                        var spId = s.ServicioProvs
                            .Where(x => x.IdServicio == s.Id && x.IdProveedor == c.Id)
                            .Select(sc => sc.Id)
                            .FirstOrDefault();

                        var spCantidad = s.ServicioProvs
                            .Where(x => x.IdServicio == s.Id && x.IdProveedor == c.Id)
                            .Select(sc => sc.Cantidad)
                            .FirstOrDefault();

                        if (spId == 0)
                        {
                            if (c.Codigo == Constantes.RUC_EULEN &&
                                s.TpoServicioNav.Codigo == Constantes.COD_SERV_VIAL)
                                spCantidad = s.Cantidad;
                            else if (c.Codigo == Constantes.RUC_SEGUMAX &&
                                     s.TpoServicioNav.Codigo == Constantes.COD_SERV_RESG)
                                spCantidad = s.Cantidad;
                        }

                        return new ListarServicioProvItem
                        {
                            Id = spId,
                            IdServicio = s.Id,
                            IdProveedor = c.Id,
                            Proveedor = c.Descripcion,
                            Cantidad = spCantidad
                        };
                    }).ToList()
                });

            return servicios;
        }
        public async Task<GridResponse<ConsultarServicioResponse>> Consultar(GridRequest<ConsultarServicioQuery> grid)
        {
            var consulta = _contexto.Servicio.AsNoTracking().AsQueryable();
            var servicio = grid.Filtros;

            if (servicio.IdProveedor.HasValue)
                consulta = consulta.Where(x => x.ServicioProvs.Where(c => c.IdProveedor == servicio.IdProveedor).Any());
            if (servicio.IdTpoServicio.HasValue)
                consulta = consulta.Where(x => x.IdTpoServicio == servicio.IdTpoServicio);
            if (servicio.FechaIni.HasValue && servicio.FechaFin.HasValue)
                consulta = consulta.Where(x => x.Fecha >= servicio.FechaIni && x.Fecha <= servicio.FechaFin);
            if (!string.IsNullOrEmpty(servicio.Folio))
                consulta = consulta.Where(x => x.SolicitudNav.Folio == servicio.Folio);

            var total = await consulta.SelectMany(x => x.ServicioProvs
                       .Where(s => !servicio.IdProveedor.HasValue
                                || s.IdProveedor == servicio.IdProveedor))
                        .CountAsync();

            var data = await consulta
            .OrderByDescending(x => x.FechaReg)
            .Skip((grid.Page - 1) * grid.PageSize)
            .Take(grid.PageSize)
            .SelectMany(
                x => x.ServicioProvs
                       .Where(s => !servicio.IdProveedor.HasValue || s.IdProveedor == servicio.IdProveedor),
                (x, c) => new ConsultarServicioResponse
                {
                    Id = c.Id,
                    Folio = x.SolicitudNav.Folio,
                    TpoServicio = x.TpoServicioNav.Descripcion,
                    Fecha = x.Fecha,
                    HraInicio = x.HraInicio,
                    HraFinal = x.HraFinal,
                    DiaSig = x.DiaSig,
                    Coordenada = x.Coordenada,
                    Direccion = x.Direccion,
                    Cantidad = c.Cantidad,
                    Proveedor = c.ProveedorNav.Descripcion,
                    Estado = c.Estado,
                    Asignar = c.Estado == Constantes.COD_ESTADO_SERVPROV_CONFIRMADO,
                    Cerrar = c.Estado == Constantes.COD_ESTADO_SERVPROV_ASIGNADO,
                    Visual = c.Estado == Constantes.COD_ESTADO_SERVPROV_CERRADO,
                    Ampliar = c.Estado == Constantes.COD_ESTADO_SERVPROV_ASIGNADO,
                    Aprobar = c.Efectivos.Any(e => e.EstAmplia == Constantes.AMPL_PENDIENTE)
                }
            ).ToListAsync();

            return new GridResponse<ConsultarServicioResponse>
            {
                Data = data,
                Total = total
            };
        }
    }
}