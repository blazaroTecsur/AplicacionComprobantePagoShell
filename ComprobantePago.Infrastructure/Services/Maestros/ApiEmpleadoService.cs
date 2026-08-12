using ComprobantePago.Application.DTOs.Comprobante.Common;
using ComprobantePago.Application.Interfaces.Services.Maestros;
using Maestro.Abstractions.Interfaces;

namespace ComprobantePago.Infrastructure.Services.Maestros
{
    public class ApiEmpleadoService(IMaestrosProveedorService maestros) : IEmpleadoService
    {
        public async Task<IEnumerable<ComboDto>> ObtenerEmpleadosAsync(string filtro = "")
        {
            var result = await maestros.GetAllAsync(filtro, 1, 100);
            return result.Items
                .Where(p => p.TipoPersona == "1" && p.Estado.ToUpper() != "INACTIVO")
                .Select(p => new ComboDto
                {
                    Codigo      = p.Ruc,
                    Descripcion = p.NombreProveedor
                });
        }
    }
}
