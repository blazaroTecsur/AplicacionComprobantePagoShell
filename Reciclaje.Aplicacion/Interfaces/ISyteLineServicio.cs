using System.Text.Json;

namespace Reciclaje.Aplicacion.Interfaces;

public interface ISyteLineServicio
{
    Task<JsonElement> GetTokenAsync(string accessTokenUrl, string clientId, string clientSecret,
                                    string username, string password);

    Task<JsonElement> IdoGetAsync(string endpointUrl, string accessToken, string mongooseConfig);

    Task<JsonElement> IdoPutAsync(string endpointUrl, string accessToken,
                                   string mongooseConfig, object payload);

    Task<(int insertados, int omitidos, List<string> errores)>
        InsertIntegracionAsync(IEnumerable<JsonElement> items);

    Task<object> InsertSroAsync(IEnumerable<JsonElement> items);

    Task<object> BuildPayloadAsync(IEnumerable<int>? sroLineaIds = null);

    Task<decimal?> GetCostoUnitarioAsync(string item, string accessToken, string mongooseConfig);

}