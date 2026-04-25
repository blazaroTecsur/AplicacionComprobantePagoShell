using Microsoft.EntityFrameworkCore;
using Resguardo.Domain.Entities;
using Resguardo.Domain.Interfaces;
using Resguardo.Infrastructure.Data;

namespace Resguardo.Infrastructure.Repositorios
{
    public class RepositorioBase<T>(DBContexto contexto) : IRepositorioBase<T> where T : EntidadBase
    {
        protected readonly DBContexto _contexto = contexto;
        protected readonly DbSet<T> _entidades = contexto.Set<T>();
        public IQueryable<T> Consulta()
        {
            return _entidades.AsNoTracking();
        }
        public async Task<T> ObtenerPorId(int id)
        {
            return await _entidades.AsNoTracking().FirstAsync(e=> e.Id == id);
        }
        public async Task Insertar(T entidad)
        {
            ArgumentNullException.ThrowIfNull(entidad);
            await _entidades.AddAsync(entidad);
        }
        public async Task Actualizar(T entidad)
        {
            ArgumentNullException.ThrowIfNull(entidad);

            var local = _entidades.Local.FirstOrDefault(e => e.Id == entidad.Id);

            if (local != null)
            {
                // ✅ Copiar valores sobre el tracked existente
                _contexto.Entry(local).CurrentValues.SetValues(entidad);
            }
            else
            {
                // ✅ Cargar desde BD y aplicar cambios (más seguro)
                var enBd = await _entidades.FindAsync(entidad.Id)
                    ?? throw new KeyNotFoundException(
                        $"No se encontró {typeof(T).Name} con Id {entidad.Id}");

                _contexto.Entry(enBd).CurrentValues.SetValues(entidad);
            }

            //var local = _entidades.Local.FirstOrDefault(x => x.Id == entidad.Id);
            //if (local != null)
            //{

            //    _entidades.Entry(local).State = EntityState.Detached;
            //    _entidades.Entry(local).CurrentValues.SetValues(entidad);
            //}
            //else
            //{
            //    _entidades.Attach(entidad);
            //    _entidades.Entry(entidad).State = EntityState.Modified;
            //}
        }
        public async Task Eliminar(int id)
        {
            var entidad = await _entidades.FindAsync(id)
           ?? throw new KeyNotFoundException(
               $"No se encontró {typeof(T).Name} con Id {id}");

            _entidades.Remove(entidad);

            //T entidad = await ObtenerPorId(id);
            //var local = _entidades.Local.FirstOrDefault(e => e.Equals(entidad));
            //if (local != null)
            //{
            //    _entidades.Remove(local);
            //}
            //else
            //{
            //    _entidades.Attach(entidad);
            //    _entidades.Remove(entidad);
            //}
        }
    }
}