using Resguardo.Application.Interfaces;
using Resguardo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Resguardo.Application.Queries.ListarServicio;

namespace Resguardo.Infrastructure.QueryServices
{
    public class ServicioProvQueryService : IServicioProvQueryService
    {
        private readonly DBContexto _contexto;
        public ServicioProvQueryService(DBContexto contexto)
        {
            _contexto = contexto;
        }
        public async Task<IEnumerable<ListarServicioProvItem>> Listar(int idSolicitud)
        {
            var serviciosCtta = await _contexto.ServicioCtta
                .Where(s => s.ServicioNav.IdSolicitud == idSolicitud)
                .Select(g => new ListarServicioProvItem
            {
                Id = g.Id,
                IdServicio = g.IdServicio,
                IdProveedor = g.IdProveedor,
                Proveedor = g.ProveedorNav.Descripcion,
                Cantidad = g.Cantidad
            }).ToListAsync();

            return serviciosCtta;
        }
    }
}
