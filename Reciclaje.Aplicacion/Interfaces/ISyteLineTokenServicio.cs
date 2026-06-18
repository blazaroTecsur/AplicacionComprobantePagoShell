namespace Reciclaje.Aplicacion.Interfaces;

/// <summary>
/// Proporciona credenciales SyteLine (accessToken + mongooseConfig) listas para usar.
/// Internamente cachea el token hasta que expira o falla, evitando una llamada HTTP
/// por cada operación de negocio.
/// </summary>
public interface ISyteLineTokenServicio
{
    /// <summary>
    /// Devuelve un token válido y el mongooseConfig configurado.
    /// Si el token en caché sigue vigente lo reutiliza; si no, lo renueva.
    /// </summary>
    Task<SyteLineCredenciales> ObtenerCredencialesAsync();
}

/// <summary>Par (accessToken, mongooseConfig) listo para adjuntar a llamadas HTTP.</summary>
public record SyteLineCredenciales(string AccessToken, string MongooseConfig);
