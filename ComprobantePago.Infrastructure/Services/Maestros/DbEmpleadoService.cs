using ComprobantePago.Application.DTOs.Comprobante.Common;
using ComprobantePago.Application.Interfaces.Services.Maestros;
using ComprobantePago.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ComprobantePago.Infrastructure.Services.Maestros
{
    public class DbEmpleadoService(AppDbContext contexto) : IEmpleadoService
    {
        private readonly AppDbContext _contexto = contexto;

        public async Task<IEnumerable<ComboDto>> ObtenerEmpleadosAsync(string filtro = "")
        {
            var query = _contexto.Empleados
                .Where(x => x.Estado == null || x.Estado.ToUpper() != "INACTIVO");

            if (!string.IsNullOrWhiteSpace(filtro))
                query = query.Where(x =>
                    x.Codigo.Contains(filtro) ||
                    x.NombreCompleto.Contains(filtro));

            return await query
                .OrderBy(x => x.NombreCompleto)
                .Select(x => new ComboDto
                {
                    Codigo = x.Codigo,
                    Descripcion = $"{x.Codigo} - {x.NombreCompleto}"
                })
                .ToListAsync();
        }
    }
}
