using ComprobantePago.Application.DTOs.Comprobante.Common;
using ComprobantePago.Application.Interfaces.Services.Maestros;
using Maestro.Abstractions.Interfaces;
using Seguridad.Abstractions.Interfaces;

namespace ComprobantePago.Infrastructure.Services.Maestros
{
    public class ApiProveedorService(IMaestrosProveedorService maestros,
        IUsuarioContexto usuario) : IProveedorService
    {
        public async Task<IEnumerable<ComboDto>> ObtenerProveedoresAsync(string filtro = "")
        {
            var result = await maestros.GetAllAsync(usuario.Esquema, filtro, 1, 100);
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
