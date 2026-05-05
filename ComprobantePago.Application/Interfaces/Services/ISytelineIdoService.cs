using ComprobantePago.Application.DTOs.Infor;
using System.Text.Json;

namespace ComprobantePago.Application.Interfaces.Services
{
    /// <summary>
    /// Cliente para el servicio IDO REST de Infor Syteline (MGRestService.svc).
    /// URLs según la especificación real del servicio.
    /// </summary>
    public interface ISytelineIdoService
    {
        /// <summary>
        /// Consulta registros de un IDO.
        /// GET /json/{ido}?props=...&amp;filter=...&amp;recordCap=N&amp;orderBy=...
        /// </summary>
        Task<JsonElement> LoadAsync(
            string  ido,
            string? props     = null,
            string? filter    = null,
            int     recordCap = 0,
            string? orderBy   = null,
            bool    adv       = false,
            CancellationToken ct = default);

        /// <summary>
        /// Devuelve la definición de propiedades de un IDO (schema).
        /// GET /json/idoinfo/{ido}
        /// </summary>
        Task<JsonElement> IdoInfoAsync(
            string ido,
            CancellationToken ct = default);

        /// <summary>
        /// Invoca un método de un IDO.
        /// GET /json/method/{ido}/{method}
        /// </summary>
        Task<JsonElement> InvokeMethodAsync(
            string ido,
            string method,
            CancellationToken ct = default);

        /// <summary>
        /// Inserta un registro en un IDO usando el formato Action/ItemId/Properties.
        /// POST /json/{ido}/additem  — sin refresh
        /// POST /json/{ido}/additem/adv?refresh=PROPS&amp;props=... — con refresh de campos
        /// </summary>
        Task<JsonElement> InsertItemAsync(
            string ido,
            IEnumerable<IdoProperty> properties,
            string? refresh = null,
            string? props   = null,
            CancellationToken ct = default);

        /// <summary>
        /// Inserta múltiples registros en un IDO.
        /// POST /json/{ido}/additems
        /// </summary>
        Task<JsonElement> InsertItemsAsync(
            string ido,
            object payload,
            CancellationToken ct = default);

        /// <summary>
        /// Actualiza un registro en un IDO.
        /// PUT /json/{ido}/updateitem
        /// </summary>
        Task<JsonElement> UpdateItemAsync(
            string ido,
            object payload,
            CancellationToken ct = default);

        /// <summary>
        /// Elimina un registro de un IDO por su ItemId.
        /// POST /json/{ido}/deleteitem
        /// </summary>
        Task<JsonElement> DeleteItemAsync(
            string ido,
            string itemId,
            CancellationToken ct = default);

        /// <summary>
        /// Lista las configuraciones IDO disponibles.
        /// GET /json/configurations
        /// </summary>
        Task<JsonElement> ObtenerConfiguracionesAsync(CancellationToken ct = default);
    }
}
