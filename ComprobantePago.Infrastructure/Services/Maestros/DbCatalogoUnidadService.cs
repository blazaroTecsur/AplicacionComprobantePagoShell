using ComprobantePago.Application.DTOs.Comprobante.Common;
using ComprobantePago.Application.Interfaces.Services.Maestros;
using ComprobantePago.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ComprobantePago.Infrastructure.Services.Maestros
{
    public class DbCatalogoUnidadService(AppDbContext contexto) : ICatalogoUnidadService
    {
        private readonly AppDbContext _contexto = contexto;

        public async Task<IEnumerable<ComboDto>> ObtenerCodigosUnidadAsync(
            int unidad, string filtro = "")
        {
            bool tieneFiltro = !string.IsNullOrWhiteSpace(filtro);

            return unidad switch
            {
                1 => await _contexto.CodigosUnidad1
                    .Where(x => x.Activo &&
                        (!tieneFiltro || x.Codigo.Contains(filtro) || x.Descripcion.Contains(filtro)))
                    .OrderBy(x => x.Codigo)
                    .Select(x => new ComboDto { Codigo = x.Codigo, Descripcion = x.Descripcion })
                    .ToListAsync(),

                3 => await _contexto.CodigosUnidad3
                    .Where(x => x.Activo &&
                        (!tieneFiltro || x.Codigo.Contains(filtro) || x.Descripcion.Contains(filtro)))
                    .OrderBy(x => x.Codigo)
                    .Select(x => new ComboDto { Codigo = x.Codigo, Descripcion = x.Descripcion })
                    .ToListAsync(),

                4 => await _contexto.CodigosUnidad4
                    .Where(x => x.Activo &&
                        (!tieneFiltro || x.Codigo.Contains(filtro) || x.Descripcion.Contains(filtro)))
                    .OrderBy(x => x.Codigo)
                    .Select(x => new ComboDto { Codigo = x.Codigo, Descripcion = x.Descripcion })
                    .ToListAsync(),

                _ => new List<ComboDto>()
            };
        }
    }
}
