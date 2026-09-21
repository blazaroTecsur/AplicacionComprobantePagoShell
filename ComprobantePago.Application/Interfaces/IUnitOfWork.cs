namespace ComprobantePago.Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
        /// <summary>
        /// Ejecuta una acción dentro de una transacción compatible con la
        /// estrategia de reintentos (EnableRetryOnFailure).
        /// </summary>
        Task ExecuteInTransactionAsync(Func<Task> action);
    }
}
