using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ComprobantePago.Infrastructure.Persistence
{
    public class RepositorioBase<T>(AppDbContext contexto) where T : class
    {
        protected readonly AppDbContext _contexto = contexto;
        protected readonly DbSet<T> _entidades = contexto.Set<T>();

        public async Task<T?> ObtenerPorIdAsync(int id)
            => await _entidades.FindAsync(id);

        public async Task<IEnumerable<T>> ObtenerTodosAsync()
            => await _entidades.ToListAsync();

        public async Task<IEnumerable<T>> BuscarAsync(
            Expression<Func<T, bool>> predicado)
            => await _entidades.Where(predicado).ToListAsync();

        public async Task AgregarAsync(T entidad)
        {
            await _entidades.AddAsync(entidad);
            await _contexto.SaveChangesAsync();
        }

        public async Task ActualizarAsync(T entidad)
        {
            _entidades.Update(entidad);
            await _contexto.SaveChangesAsync();
        }

        public async Task EliminarAsync(T entidad)
        {
            _entidades.Remove(entidad);
            await _contexto.SaveChangesAsync();
        }
    }
}