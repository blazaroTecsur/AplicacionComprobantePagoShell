using Maestros.Abstractions.DTOs;

namespace Maestros.Abstractions.Interfaces
{
    public interface IMaestrosSocioService
    {
        Task<PagedResult<SocioListDto>> GetAllAsync(
            string? proveedor, string? filtro, int pagina, int tamano, CancellationToken ct = default);

        Task<SocioListDto?> GetByCodigoAsync(
            string codigo, CancellationToken ct = default);
    }
}
