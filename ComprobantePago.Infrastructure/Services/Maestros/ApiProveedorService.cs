using ComprobantePago.Application.DTOs.Comprobante.Common;
using ComprobantePago.Application.Interfaces.Services.Maestros;
using Maestros.Abstractions.Interfaces;

namespace ComprobantePago.Infrastructure.Services.Maestros
{
    public class ApiProveedorService(IMaestrosProveedorService maestros) : IProveedorService
    {
        public async Task<IEnumerable<ComboDto>> ObtenerProveedoresAsync(string filtro = "")
        {
            var result = await maestros.GetAllAsync(filtro, 1, 100);
            return result.Items
                .Where(p => p.Estado.ToUpper() != "INACTIVO")
                .Select(p => new ComboDto
                {
                    Codigo      = p.Ruc,
                    Descripcion = p.NombreProveedor,
                });
        }
    }
}
