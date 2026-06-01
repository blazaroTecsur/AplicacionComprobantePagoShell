using Maestros.Abstractions.DTOs;

namespace Maestros.Abstractions.Interfaces
{
    public interface IMaestrosProveedorService
    {
        Task<PagedResult<ProveedorListDto>> GetAllAsync(
            string? filtro, int pagina, int tamano, CancellationToken ct = default);

        Task<ProveedorDetalleDto?> GetByRucAsync(
            string ruc, CancellationToken ct = default);
    }
}
