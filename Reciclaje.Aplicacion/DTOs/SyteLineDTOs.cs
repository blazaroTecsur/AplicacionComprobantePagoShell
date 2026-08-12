using System.Text.Json;

namespace Reciclaje.Aplicacion.DTOs;

// ── DTOs para SyteLineController ────────────────────────────────────────────

/// <summary>Petición de sincronización SRO filtrada por número de SRO.</summary>
public record RunRequest(string? SroNum);

/// <summary>Credenciales OAuth2 para obtener token de SyteLine.</summary>
public record TokenRequest(
    string AccessTokenUrl,
    string ClientId,
    string ClientSecret,
    string Username,
    string Password);

/// <summary>Parámetros para consulta IDO GET.</summary>
public record IdoRequest(
    string EndpointUrl,
    string AccessToken,
    string MongooseConfig);

/// <summary>Parámetros para actualización IDO PUT.</summary>
public record IdoPutRequest(
    string EndpointUrl,
    string AccessToken,
    string MongooseConfig,
    object Body);

/// <summary>Lista de ítems JSON para operaciones de inserción masiva.</summary>
public record ItemsRequest(JsonElement[]? Items);

/// <summary>IDs de líneas SRO para construir el payload IDO.</summary>
public record BuildPayloadRequest(IEnumerable<int>? SroLineaIds);
