using Reciclaje.Aplicacion.DTOs.Reciclaje;

namespace Reciclaje.Aplicacion.Interfaces
{
    /// <summary>
    /// Servicio encargado de la lógica de negocio para la vista Index:
    /// búsqueda de líneas SRO y generación de Vale Específico / Consolidado.
    /// </summary>
    public interface IValeRecuperoGeneracionServicio
    {
        /// <summary>Busca líneas SRO según los filtros indicados.</summary>
        Task<ValeRecuperoViewModel> Buscar(ValeRecuperoBusquedaDto filtros);

        /// <summary>Genera un vale independiente por cada línea seleccionada.</summary>
        Task GenerarValeEspecifico(List<int> srolineaIds, string usuarioActual);

        /// <summary>Genera un único vale que consolida todas las líneas seleccionadas.</summary>
        Task GenerarValeConsolidado(List<int> srolineaIds, string usuarioActual);

        /// <summary>
        /// Retorna los IDs (como string identificador) de las líneas seleccionadas
        /// cuyo EstadoLinea sea "Procesado", para bloquear la generación del vale.
        /// </summary>
        Task<List<string>> ObtenerLineasProcesadas(List<int> srolineaIds);
    }
}
