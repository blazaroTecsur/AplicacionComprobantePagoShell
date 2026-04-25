using Resguardo.Domain.Repositories;

namespace Resguardo.Domain.Interfaces
{
    public interface IUnidadTrabajo : IAsyncDisposable
    {
        public ISolicitudRepositorio SolicitudRepositorio { get; }
        public IServicioRepositorio ServicioRepositorio { get; }
        public IEfectivoRepositorio EfectivoRepositorio { get; }
        public IGenericoRepositorio GenericoRepositorio { get; }
        public IPersonalRepositorio PersonalRepositorio { get; }
        public IServicioProvRepositorio ServicioProvRepositorio { get; }
        public IConfigRepositorio ConfigRepositorio { get; }
        Task SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
