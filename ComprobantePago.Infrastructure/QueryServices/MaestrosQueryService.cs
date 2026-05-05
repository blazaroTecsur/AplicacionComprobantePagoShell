using ComprobantePago.Application.DTOs.Comprobante.Common;
using ComprobantePago.Application.Interfaces.QueryServices;
using ComprobantePago.Application.Interfaces.Services.Maestros;
using ComprobantePago.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ComprobantePago.Infrastructure.QueryServices
{
    public class MaestrosQueryService(
        AppDbContext contexto,
        IEmpleadoService empleadoService,
        ICatalogoUnidadService cataloService,
        ICuentaContableService cuentaService)
        : IMaestrosQueryService
    {
        private readonly AppDbContext _contexto = contexto;
        private readonly IEmpleadoService _empleadoService = empleadoService;
        private readonly ICatalogoUnidadService _cataloService = cataloService;
        private readonly ICuentaContableService _cuentaService = cuentaService;

        public async Task<IEnumerable<ComboDto>> ObtenerTiposDocumentoAsync()
            => await _contexto.TiposDocumento
                .Where(x => x.Activo)
                .OrderBy(x => x.Codigo)
                .Select(x => new ComboDto { Codigo = x.Codigo, Descripcion = x.Descripcion })
                .ToListAsync();

        public async Task<IEnumerable<ComboDto>> ObtenerTiposSunatAsync()
            => await _contexto.TiposSunat
                .Where(x => x.Activo)
                .OrderBy(x => x.Codigo)
                .Select(x => new ComboDto { Codigo = x.Codigo, Descripcion = x.Descripcion })
                .ToListAsync();

        public async Task<IEnumerable<ComboDto>> ObtenerMonedasAsync()
            => await _contexto.Monedas
                .Where(x => x.Activo)
                .OrderBy(x => x.Codigo)
                .Select(x => new ComboDto { Codigo = x.Codigo, Descripcion = x.Descripcion })
                .ToListAsync();

        public async Task<IEnumerable<ComboDto>> ObtenerLugaresPagoAsync()
            => await _contexto.LugaresPago
                .Where(x => x.Activo)
                .OrderBy(x => x.Codigo)
                .Select(x => new ComboDto { Codigo = x.Codigo, Descripcion = x.Descripcion })
                .ToListAsync();

        public async Task<IEnumerable<ComboDto>> ObtenerTiposDetraccionAsync()
            => await _contexto.TiposDetraccion
                .Where(x => x.Activo)
                .OrderBy(x => x.Codigo)
                .Select(x => new ComboDto
                {
                    Codigo = x.Codigo,
                    Descripcion = $"{x.Codigo} - {x.Descripcion}",
                    Porcentaje = x.Porcentaje
                })
                .ToListAsync();

        public async Task<IEnumerable<ComboDto>> ObtenerEstadosAsync()
            => await _contexto.EstadosComprobante
                .Where(x => x.Activo)
                .OrderBy(x => x.Codigo)
                .Select(x => new ComboDto { Codigo = x.Codigo, Descripcion = x.Descripcion })
                .ToListAsync();

        public Task<IEnumerable<ComboDto>> ObtenerEmpleadosAsync(string filtro = "")
            => _empleadoService.ObtenerEmpleadosAsync(filtro);

        public Task<IEnumerable<ComboDto>> ObtenerCuentasContablesAsync(string filtro = "")
            => _cuentaService.ObtenerCuentasContablesAsync(filtro);

        public Task<IEnumerable<ComboDto>> ObtenerCodigosUnidadAsync(
            string campo, int unidad, string codigo, string filtro = "")
            => _cataloService.ObtenerCodigosUnidadAsync(unidad, filtro);
    }
}
