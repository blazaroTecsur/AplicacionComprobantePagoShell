using ComprobantePago.Application.DTOs.Comprobante.Common;
using ComprobantePago.Application.Interfaces.Services.Maestros;
using Maestros.Abstractions.Interfaces;

namespace ComprobantePago.Infrastructure.Services.Maestros
{
    public class ApiEmpleadoService(IMaestrosEmpleadoService maestros) : IEmpleadoService
    {
        public async Task<IEnumerable<ComboDto>> ObtenerEmpleadosAsync(string filtro = "")
        {
            var result = await maestros.GetAllAsync(filtro, 1, 100);
            return result.Items.Select(e => new ComboDto
            {
                Codigo      = e.Codigo,
                Descripcion = e.NombreCompleto,
            });
        }
    }
}
