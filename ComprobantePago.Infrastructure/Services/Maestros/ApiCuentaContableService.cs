using ComprobantePago.Application.DTOs.Comprobante.Common;
using ComprobantePago.Application.Interfaces.Services.Maestros;
using Maestros.Abstractions.Interfaces;

namespace ComprobantePago.Infrastructure.Services.Maestros
{
    public class ApiCuentaContableService(IMaestrosCuentaContableService maestros) : ICuentaContableService
    {
        public async Task<IEnumerable<ComboDto>> ObtenerCuentasContablesAsync(string filtro = "")
        {
            var result = await maestros.GetAllAsync(filtro, 1, 100);
            return result.Items
                .Where(c => c.Codigo.Length >= 7)
                .Select(c => new ComboDto
                {
                    Codigo      = c.Codigo,
                    Descripcion = c.Descripcion,
                });
        }
    }
}
