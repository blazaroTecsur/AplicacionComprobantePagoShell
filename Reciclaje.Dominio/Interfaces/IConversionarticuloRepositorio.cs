using Reciclaje.Dominio.Entidades;

namespace Reciclaje.Dominio.Interfaces
{
    public interface IConversionarticuloRepositorio<T>
    {
        Task<T> ObtenerPorId(int id);
        Task Insertar(T entidad);
        void Actualizar(T entidad);
        Task Eliminar(int id);
        Task<IEnumerable<Conversionarticulo>> Listar();
        Task<Conversionarticulo?> ObtenerPorArticuloReciclaje(string codigoArticuloReciclaje);
    }
}