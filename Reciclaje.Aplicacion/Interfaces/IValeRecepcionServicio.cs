using Reciclaje.Aplicacion.DTOs.Reciclaje;

namespace Reciclaje.Aplicacion.Interfaces
{
    /// <summary>
    /// Servicio encargado de la lógica de negocio para la vista RecepcionarVale:
    /// búsqueda de vales en estado Pendiente y registro de recepción.
    /// </summary>
    public interface IValeRecepcionServicio
    {
        /// <summary>Busca vales según los filtros indicados (estado Pendiente).</summary>
        Task<ValeRecuperoListaViewModel> BuscarVales(ValeRecuperoBuscarDto filtros);

        /// <summary>Persiste la recepción de uno o varios vales.</summary>
        Task GuardarRecepcion(List<ValeRecepcionDto> recepciones, string usuarioActual);
    }
}
