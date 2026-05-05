using ComprobantePago.Application.DTOs.Comprobante.Common;

namespace ComprobantePago.Application.Interfaces.Services.Maestros
{
    public interface ICuentaContableService
    {
        Task<IEnumerable<ComboDto>> ObtenerCuentasContablesAsync(string filtro = "");
    }
}
