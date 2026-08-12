using Reciclaje.Aplicacion.DTOs.Reciclaje;

namespace Reciclaje.Aplicacion.Interfaces
{
    public interface ITareaOrdenCompraServicio
    {
        Task<IEnumerable<TareaOrdenCompraDto>> ObtenerTodos();
        Task<TareaOrdenCompraEditarDto> ObtenerParaEditar(int id);
        Task<(bool exitoso, string mensaje)> Crear(TareaOrdenCompraCrearDto dto, string usuarioActual);
        Task Editar(TareaOrdenCompraEditarDto dto, string usuarioActual);
        Task Eliminar(int id);
        

    }
}
