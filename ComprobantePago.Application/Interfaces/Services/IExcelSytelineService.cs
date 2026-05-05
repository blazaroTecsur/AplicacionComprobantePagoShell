using ComprobantePago.Application.DTOs.Responses;

namespace ComprobantePago.Application.Interfaces.Services
{
    public interface IExcelSytelineService
    {
        byte[] GenerarCabecera(
            IEnumerable<SytelineCabeceraDto> datos);
        byte[] GenerarDistribucion(
            IEnumerable<SytelineDistribucionDto> datos);
    }
}
