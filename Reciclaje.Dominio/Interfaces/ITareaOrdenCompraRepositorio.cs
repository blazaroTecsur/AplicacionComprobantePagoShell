using Reciclaje.Dominio.Entidades;

namespace Reciclaje.Dominio.Interfaces
{
    public interface ITareaOrdenCompraRepositorio
    {
        Task<IEnumerable<Tareaordencompra>> ObtenerTodos();
        Task<Tareaordencompra?> ObtenerPorId(int id);
        Task<Tareaordencompra?> ObtenerPorNombrePo(string nombrePo);
        Task<bool> ExistePorAnnoMesSitio(short anno, byte mes, string? sitio, int? excluirId = null);
        Task<bool> ExistePeriodoActual();
        Task Insertar(Tareaordencompra tarea);
        Task Actualizar(Tareaordencompra tarea);
        Task Eliminar(Tareaordencompra tarea);
    }
}