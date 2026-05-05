using ComprobantePago.Application.DTOs.Comprobante.Common;
using ComprobantePago.Application.Interfaces.Services.Maestros;
using ComprobantePago.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ComprobantePago.Infrastructure.Services.Maestros
{
    public class DbProveedorService(AppDbContext contexto) : IProveedorService
    {
        private readonly AppDbContext _contexto = contexto;

        public async Task<IEnumerable<ComboDto>> ObtenerProveedoresAsync(string filtro = "")
        {
            var query = _contexto.Proveedores
                .Where(x => x.Estado.ToUpper() != "INACTIVO");

            if (!string.IsNullOrWhiteSpace(filtro))
                query = query.Where(x =>
                    x.Ruc.Contains(filtro) ||
                    (x.NombreProveedor != null && x.NombreProveedor.Contains(filtro)));

            return await query
                .OrderBy(x => x.NombreProveedor)
                .Select(x => new ComboDto
                {
                    Codigo = x.Ruc,
                    Descripcion = $"{x.Ruc} - {x.NombreProveedor}"
                })
                .ToListAsync();
        }
    }
}
