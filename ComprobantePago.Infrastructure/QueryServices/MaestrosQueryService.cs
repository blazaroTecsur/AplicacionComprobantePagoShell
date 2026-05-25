using ComprobantePago.Application.DTOs.Comprobante.Common;
using ComprobantePago.Application.Interfaces.QueryServices;
using ComprobantePago.Application.Interfaces.Services.Maestros;
using ComprobantePago.Infrastructure.Persistence;
using Infor.Abstractions.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ComprobantePago.Infrastructure.QueryServices
{
    public class MaestrosQueryService(
        AppDbContext contexto,
        IEmpleadoService empleadoService,
        ICatalogoUnidadService cataloService,
        ICuentaContableService cuentaService,
        IInforIdoService ido,
        ILogger<MaestrosQueryService> logger)
        : IMaestrosQueryService
    {
        private readonly AppDbContext _contexto = contexto;
        private readonly IEmpleadoService _empleadoService = empleadoService;
        private readonly ICatalogoUnidadService _cataloService = cataloService;
        private readonly ICuentaContableService _cuentaService = cuentaService;
        private readonly IInforIdoService _ido = ido;
        private readonly ILogger<MaestrosQueryService> _logger = logger;

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

        public async Task<decimal> ObtenerTipoCambioAsync(string moneda, DateTime fecha)
        {
            try
            {
                var fechaFmt = fecha.ToString("yyyyMMdd") + " 00:00:00.000";
                var resultado = await _ido.LoadAsync(
                    ido:    "SLCurrates",
                    props:  "FromCurrCode,SellRate",
                    filter: $"EffDate = '{fechaFmt}' AND FromCurrCode='{moneda}'",
                    adv:    true);

                if (resultado.TryGetProperty("Items", out var items) && items.GetArrayLength() > 0)
                {
                    // Items es array de arrays: cada fila = [{Name,Value}, ...]
                    foreach (var campo in items[0].EnumerateArray())
                    {
                        if (campo.TryGetProperty("Name",  out var nombre) &&
                            nombre.GetString() == "SellRate" &&
                            campo.TryGetProperty("Value", out var valor))
                        {
                            return decimal.TryParse(
                                valor.GetString(),
                                System.Globalization.NumberStyles.Any,
                                System.Globalization.CultureInfo.InvariantCulture,
                                out var tasa) ? tasa : 0m;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo obtener tipo de cambio de Syteline para {Moneda} {Fecha}", moneda, fecha);
            }
            return 0m;
        }
    }
}
