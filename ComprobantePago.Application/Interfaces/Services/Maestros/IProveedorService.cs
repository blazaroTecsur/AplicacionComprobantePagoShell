using ComprobantePago.Application.DTOs.Comprobante.Common;

namespace ComprobantePago.Application.Interfaces.Services.Maestros
{
    public interface IProveedorService
    {
        Task<IEnumerable<ComboDto>> ObtenerProveedoresAsync(string filtro = "");
    }
}
