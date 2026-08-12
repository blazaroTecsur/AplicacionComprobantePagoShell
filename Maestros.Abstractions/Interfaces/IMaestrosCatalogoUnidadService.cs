using Maestros.Abstractions.DTOs;

namespace Maestros.Abstractions.Interfaces
{
    public interface IMaestrosCatalogoUnidadService
    {
        Task<PagedResult<CodUnidadListDto>> GetByUnidadAsync(
            int unidad, string empresa, string? inicial, string? filtro, int pagina, int tamano,
            CancellationToken ct = default);
    }
}
