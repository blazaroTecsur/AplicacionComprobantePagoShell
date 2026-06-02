using Maestros.Abstractions.DTOs;

namespace Maestros.Abstractions.Interfaces
{
    public interface IMaestrosEmpleadoService
    {
        Task<PagedResult<EmpleadoListDto>> GetAllAsync(
            string? filtro, int pagina, int tamano, CancellationToken ct = default);
    }
}
