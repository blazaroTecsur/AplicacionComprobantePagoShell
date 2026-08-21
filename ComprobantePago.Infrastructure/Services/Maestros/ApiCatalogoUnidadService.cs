using ComprobantePago.Application.DTOs.Comprobante.Common;
using ComprobantePago.Application.Interfaces.Services.Maestros;
using ComprobantePago.Infrastructure.Extensions;
using Maestro.Abstractions.Interfaces;
using Seguridad.Abstractions.Interfaces;

namespace ComprobantePago.Infrastructure.Services.Maestros
{
    public class ApiCatalogoUnidadService(
        IMaestrosCatalogoUnidadService maestros,
        IUsuarioContexto usuario) : ICatalogoUnidadService
    {
        public async Task<IEnumerable<ComboDto>> ObtenerCodigosUnidadAsync(int unidad, string filtro = "")
        {
            // Unidad 4 es global — sin filtro de empresa (igual que DbCatalogoUnidadService)
            var empresa = unidad == 4 ? string.Empty : usuario.CodigoEmpresa();
            var result  = await maestros.GetAllAsync(usuario.Esquema, unidad, empresa, "", filtro, 1, 100);
            return result.Items.Select(c => new ComboDto
            {
                Codigo      = c.Codigo,
                Descripcion = c.Descripcion,
            });
        }
    }
}
