using ComprobantePago.Application.DTOs.Responses;

namespace ComprobantePago.Application.Interfaces.QueryServices
{
    public interface ISytelineQueryService
    {
        Task<IEnumerable<SytelineCabeceraDto>> ObtenerCabecerasSytelineAsync(List<string>? folios = null);
        Task<IEnumerable<SytelineDistribucionDto>> ObtenerDistribucionSytelineAsync(List<string>? folios = null);
    }
}
