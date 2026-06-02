using ComprobantePago.Application.DTOs.Comprobante.Common;
using ComprobantePago.Application.Interfaces.Services.Maestros;
using Maestros.Abstractions.Interfaces;
using Seguridad.Abstractions.Interfaces;

namespace ComprobantePago.Infrastructure.Services.Maestros
{
    public class ApiCatalogoUnidadService(
        IMaestrosCatalogoUnidadService maestros,
        IUsuarioContexto usuario) : ICatalogoUnidadService
    {
        public async Task<IEnumerable<ComboDto>> ObtenerCodigosUnidadAsync(int unidad, string filtro = "")
        {
            var result = await maestros.GetByUnidadAsync(unidad, usuario.Empresa, filtro, 1, 100);
            return result.Items.Select(c => new ComboDto
            {
                Codigo      = c.Codigo,
                Descripcion = c.Descripcion,
            });
        }
    }
}
