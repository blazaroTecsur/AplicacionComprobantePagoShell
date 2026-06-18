using Reciclaje.Aplicacion.DTOs.Reciclaje;

namespace Reciclaje.Aplicacion.Interfaces
{
    /// <summary>
    /// Servicio encargado de la lógica de negocio para la vista ConfirmarVale:
    /// búsqueda de vales Recepcionados/Confirmados, confirmación y rechazo.
    /// </summary>
    public interface IValeConfirmacionServicio
    {
        /// <summary>
        /// Busca vales en estado Recepcionado o Confirmado según los filtros.
        /// </summary>
        Task<ValeConfirmacionListaViewModel> BuscarValesConfirmacion(ValeRecuperoBuscarDto filtros);

        /// <summary>
        /// Confirma los vales marcados: persiste en BD y envía línea a SyteLine.
        /// </summary>
        Task GuardarConfirmacion(List<ValeConfirmacionGuardarDto> confirmaciones, string usuarioActual);

        /// <summary>
        /// Rechaza los vales seleccionados, regresándolos al estado Pendiente.
        /// </summary>
        Task RechazarVales(List<ValeConfirmacionGuardarDto> confirmaciones, string usuarioActual);
    }
}
