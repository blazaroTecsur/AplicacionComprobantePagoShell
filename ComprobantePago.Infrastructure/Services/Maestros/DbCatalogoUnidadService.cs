using ComprobantePago.Application.DTOs.Comprobante.Common;
using ComprobantePago.Application.Interfaces.Services.Maestros;
using ComprobantePago.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Seguridad.Abstractions.Interfaces;

namespace ComprobantePago.Infrastructure.Services.Maestros
{
    public class DbCatalogoUnidadService(AppDbContext contexto, IUsuarioContexto usuario) : ICatalogoUnidadService
    {
        private readonly AppDbContext _contexto = contexto;
        private readonly IUsuarioContexto _usuario = usuario;

        public async Task<IEnumerable<ComboDto>> ObtenerCodigosUnidadAsync(
            int unidad, string filtro = "")
        {
            bool tieneFiltro = !string.IsNullOrWhiteSpace(filtro);
            var empresa = _usuario.Empresa;
            bool filtrarEmpresa = !string.IsNullOrWhiteSpace(empresa);

            return unidad switch
            {
                1 => await _contexto.CodigosUnidad1
                    .Where(x => x.Activo &&
                        (!filtrarEmpresa || x.Empresa == empresa) &&
                        (!tieneFiltro || x.Codigo.Contains(filtro) || x.Descripcion.Contains(filtro)))
                    .OrderBy(x => x.Codigo)
                    .Select(x => new ComboDto { Codigo = x.Codigo, Descripcion = x.Descripcion })
                    .ToListAsync(),

                3 => await _contexto.CodigosUnidad3
                    .Where(x => x.Activo &&
                        (!filtrarEmpresa || x.Empresa == empresa) &&
                        (!tieneFiltro || x.Codigo.Contains(filtro) || x.Descripcion.Contains(filtro)))
                    .OrderBy(x => x.Codigo)
                    .Select(x => new ComboDto { Codigo = x.Codigo, Descripcion = x.Descripcion })
                    .ToListAsync(),

                2 => await _contexto.CodigosUnidad2
                    .Where(x => x.Activo &&
                        (!filtrarEmpresa || x.Empresa == empresa) &&
                        (!tieneFiltro || x.Codigo.Contains(filtro) || x.Descripcion.Contains(filtro)))
                    .OrderBy(x => x.Codigo)
                    .Select(x => new ComboDto { Codigo = x.Codigo, Descripcion = x.Descripcion })
                    .ToListAsync(),

                // Unidad 4 no tiene empresa — global para todas
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
