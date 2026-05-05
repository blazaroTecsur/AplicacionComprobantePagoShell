using ComprobantePago.Application.DTOs.Comprobante.Common;

namespace ComprobantePago.Application.Interfaces.QueryServices
{
    public interface IMaestrosQueryService
    {
        Task<IEnumerable<ComboDto>> ObtenerTiposDocumentoAsync();
        Task<IEnumerable<ComboDto>> ObtenerTiposSunatAsync();
        Task<IEnumerable<ComboDto>> ObtenerMonedasAsync();
        Task<IEnumerable<ComboDto>> ObtenerLugaresPagoAsync();
        Task<IEnumerable<ComboDto>> ObtenerTiposDetraccionAsync();
        Task<IEnumerable<ComboDto>> ObtenerEstadosAsync();
        Task<IEnumerable<ComboDto>> ObtenerEmpleadosAsync(string filtro = "");
        Task<IEnumerable<ComboDto>> ObtenerCuentasContablesAsync(string filtro = "");
        Task<IEnumerable<ComboDto>> ObtenerCodigosUnidadAsync(string campo, int unidad, string codigo, string filtro = "");
    }
}
