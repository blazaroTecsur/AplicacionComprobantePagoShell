using ComprobantePago.Application.DTOs.Comprobante.Common;
using ComprobantePago.Application.Interfaces.Services.Maestros;
using ComprobantePago.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ComprobantePago.Infrastructure.Services.Maestros
{
    public class DbCuentaContableService(AppDbContext contexto) : ICuentaContableService
    {
        private readonly AppDbContext _contexto = contexto;

        public async Task<IEnumerable<ComboDto>> ObtenerCuentasContablesAsync(string filtro = "")
        {
            var query = _contexto.CuentasContables.Where(x => x.Activo);

            if (!string.IsNullOrWhiteSpace(filtro))
                query = query.Where(x =>
                    x.Codigo.Contains(filtro) || x.Descripcion.Contains(filtro));

            return await query
                .OrderBy(x => x.Codigo)
                .Select(x => new ComboDto { Codigo = x.Codigo, Descripcion = x.Descripcion })
                .ToListAsync();
        }
    }
}
