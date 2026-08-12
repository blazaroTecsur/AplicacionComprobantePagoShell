using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Reciclaje.Aplicacion.Configuracion;
using Reciclaje.Aplicacion.Interfaces;

namespace Reciclaje.Aplicacion.Servicios;

/// <summary>
/// Obtiene y cachea el token OAuth2 de SyteLine.
///
/// — El token se renueva automáticamente cuando queda menos de
///   <see cref="MargenRenovacionSegundos"/> de vida (por defecto 60 s).
/// — Si la respuesta no trae "expires_in", se asume una vida de 1 hora.
/// — Registrado como SINGLETON en DI para que la caché sea compartida por
///   todos los servicios durante el ciclo de vida de la aplicación.
///
/// NOTA: Como este servicio es Singleton pero ISyteLineServicio es Scoped,
/// se usa IServiceScopeFactory para crear un scope temporal en cada renovación
/// de token, evitando el anti-patrón "Scoped service inside Singleton".
/// </summary>
public sealed class SyteLineTokenServicio : ISyteLineTokenServicio
{
    private const int MargenRenovacionSegundos = 60;
    private const int VidaDefectoSegundos = 3600;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SyteLineConfig _cfg;
    private readonly ILogger<SyteLineTokenServicio> _logger;

    // ── Estado de caché (solo accedido dentro del semáforo) ──────────────────
    private readonly SemaphoreSlim _semaforo = new(1, 1);
    private string? _tokenCacheado;
    private DateTime _expiracion = DateTime.MinValue;

    public SyteLineTokenServicio(
        IServiceScopeFactory scopeFactory,
        IOptions<SyteLineConfig> options,
        ILogger<SyteLineTokenServicio> logger)
    {
        _scopeFactory = scopeFactory;
        _cfg = options.Value;
        _logger = logger;
    }

    public async Task<SyteLineCredenciales> ObtenerCredencialesAsync()
    {
        // Ruta rápida sin lock: el token sigue vigente
        if (_tokenCacheado is not null && DateTime.UtcNow < _expiracion)
            return BuildCredenciales(_tokenCacheado);

        // Ruta lenta: renovar token con exclusión mutua para evitar
        // múltiples llamadas simultáneas al endpoint OAuth2
        await _semaforo.WaitAsync();
        try
        {
            // Segunda comprobación dentro del lock (otro hilo pudo haber renovado ya)
            if (_tokenCacheado is not null && DateTime.UtcNow < _expiracion)
                return BuildCredenciales(_tokenCacheado);

            _logger.LogInformation("Renovando token OAuth2 de SyteLine...");

            // Creamos un scope temporal porque ISyteLineServicio es Scoped
            // y este servicio es Singleton
            using var scope = _scopeFactory.CreateScope();
            var syteLineServicio = scope.ServiceProvider.GetRequiredService<ISyteLineServicio>();

            var tokenEl = await syteLineServicio.GetTokenAsync(
                accessTokenUrl: _cfg.AccessTokenUrl,
                clientId: _cfg.ClientId,
                clientSecret: _cfg.ClientSecret,
                username: _cfg.Username,
                password: _cfg.Password);

            if (!tokenEl.TryGetProperty("access_token", out var tokenProp))
                throw new InvalidOperationException(
                    "SyteLine no devolvió 'access_token' en la respuesta OAuth2.");

            _tokenCacheado = tokenProp.GetString()
                ?? throw new InvalidOperationException("El access_token recibido es null.");

            // Calcular vencimiento: usamos "expires_in" si viene, o el valor por defecto
            int vidaSegundos = VidaDefectoSegundos;
            if (tokenEl.TryGetProperty("expires_in", out var expProp) &&
                expProp.TryGetInt32(out var expVal))
                vidaSegundos = expVal;

            _expiracion = DateTime.UtcNow.AddSeconds(vidaSegundos - MargenRenovacionSegundos);

            _logger.LogInformation(
                "Token SyteLine renovado. Válido hasta {Expiracion:HH:mm:ss} UTC.", _expiracion);

            return BuildCredenciales(_tokenCacheado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener token OAuth2 de SyteLine.");
            throw;
        }
        finally
        {
            _semaforo.Release();
        }
    }

    private SyteLineCredenciales BuildCredenciales(string token) =>
        new(token, _cfg.MongooseConfig);
}
