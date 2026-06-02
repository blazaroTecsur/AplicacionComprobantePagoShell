using Maestros.Abstractions.DTOs;

namespace Maestros.Abstractions.Interfaces
{
    public interface IMaestrosCuentaContableService
    {
        Task<PagedResult<CuentaContableListDto>> GetAllAsync(
            string? filtro, int pagina, int tamano, CancellationToken ct = default);
    }
}
