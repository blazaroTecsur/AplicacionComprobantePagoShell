using ComprobantePago.Application.DTOs.Comprobante.Common;

namespace ComprobantePago.Application.Interfaces.Services.Maestros
{
    public interface IEmpleadoService
    {
        Task<IEnumerable<ComboDto>> ObtenerEmpleadosAsync(string filtro = "");
    }
}
