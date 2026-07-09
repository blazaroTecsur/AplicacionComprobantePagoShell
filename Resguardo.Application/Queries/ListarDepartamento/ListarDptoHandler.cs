using Maestros.Abstractions.Interfaces;
using Resguardo.Application.Common;

namespace Resguardo.Application.Queries.ListarDepartamento
{
    public class ListarDptoHandler
    {
        private readonly IMaestrosCatalogoUnidadService _unidad;
        public ListarDptoHandler(IMaestrosCatalogoUnidadService unidad)
        {
            _unidad = unidad;
        }
        public async Task<IEnumerable<ListarDptoResponse>> Ejecutar()
        {
            var unidades = await _unidad.GetByUnidadAsync(Constantes.API_UND_1, Constantes.SITE_TECSUR, Constantes.INI_UND_GOP, string.Empty, 1, 30);
            var dptos = new List<ListarDptoResponse>();

            if (unidades is not null && unidades.Total > 0)
                dptos = unidades.Items
                    .Select(u => new ListarDptoResponse { Codigo = u.Codigo, Nombre = $"{u.Codigo} - {u.Descripcion}" })
                    .OrderBy(x => x.Codigo)
                    .ToList();

            return dptos;
        }
    }
}