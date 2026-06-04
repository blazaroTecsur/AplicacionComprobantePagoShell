using Infor.Abstractions.DTOs;
using System.Text.Json;

namespace Infor.Abstractions.Interfaces
{
    /// <summary>
    /// Cliente para el servicio IDO REST de Infor Syteline (SyteLineRESTv2).
    /// </summary>
    public interface IInforIdoService
    {
        /// <summary>
        /// Consulta registros de un IDO.
        /// GET /load/{ido}?properties=...&amp;filter=...&amp;recordCap=N&amp;orderBy=...
        /// </summary>
        Task<JsonElement> LoadAsync(
            string ido,
            string? props = null,
            string? filter = null,
            int recordCap = 0,
            string? orderBy = null,
            CancellationToken ct = default);

        /// <summary>
        /// Devuelve la definición de propiedades de un IDO (schema).
        /// GET /info/{ido}
        /// </summary>
        Task<JsonElement> IdoInfoAsync(
            string ido,
            CancellationToken ct = default);

        /// <summary>
        /// Invoca un método de un IDO.
        /// POST /invoke/{ido}?method={method}  body: string[]
        /// </summary>
        Task<JsonElement> InvokeMethodAsync(
            string ido,
            string method,
            IEnumerable<string>? parameters = null,
            CancellationToken ct = default);

        /// <summary>
        /// Inserta un registro en un IDO.
        /// POST /update/{ido}  body: UpdateCollectionRequest con Action=1
        /// </summary>
        Task<JsonElement> InsertItemAsync(
            string ido,
            IEnumerable<IdoProperty> properties,
            bool refreshAfterSave = false,
            CancellationToken ct = default);

        /// <summary>
        /// Inserta múltiples registros en un IDO.
        /// POST /update/{ido}  body: UpdateCollectionRequest con múltiples Changes
        /// </summary>
        Task<JsonElement> InsertItemsAsync(
            string ido,
            object payload,
            CancellationToken ct = default);

        /// <summary>
        /// Actualiza un registro en un IDO.
        /// POST /update/{ido}  body: UpdateCollectionRequest con Action=2
        /// </summary>
        Task<JsonElement> UpdateItemAsync(
            string ido,
            object payload,
            CancellationToken ct = default);

        /// <summary>
        /// Elimina un registro de un IDO por su ItemId.
        /// POST /update/{ido}  body: UpdateCollectionRequest con Action=4
        /// </summary>
        Task<JsonElement> DeleteItemAsync(
            string ido,
            string itemId,
            CancellationToken ct = default);

        /// <summary>
        /// Lista las configuraciones IDO disponibles.
        /// GET /configurations
        /// </summary>
        Task<JsonElement> ObtenerConfiguracionesAsync(CancellationToken ct = default);
    }
}
