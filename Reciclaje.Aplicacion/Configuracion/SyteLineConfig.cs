using System.ComponentModel.DataAnnotations;

namespace Reciclaje.Aplicacion.Configuracion;

/// <summary>
/// Representa la sección "SyteLine" de la configuración.
///
/// Valores SENSIBLES (vacíos en appsettings.json):
///   Se inyectan desde variables de entorno del sistema o appsettings.Production.json.
///   Nunca deben estar en el repositorio Git.
///
/// Valores NO SENSIBLES:
///   Pueden vivir en appsettings.json sin problema.
/// </summary>
public sealed class SyteLineConfig
{
    /// <summary>Nombre de la sección en appsettings.json</summary>
    public const string Seccion = "SyteLine";

    // ── Credenciales OAuth2 (SENSIBLES) ────────────────────────────────────

    [Required(ErrorMessage = "SyteLine:AccessTokenUrl es obligatorio. " +
        "Configúrelo en appsettings.Production.json o como variable de entorno " +
        "SyteLine__AccessTokenUrl")]
    public string AccessTokenUrl { get; init; } = string.Empty;

    [Required(ErrorMessage = "SyteLine:ClientId es obligatorio.")]
    public string ClientId { get; init; } = string.Empty;

    [Required(ErrorMessage = "SyteLine:ClientSecret es obligatorio.")]
    public string ClientSecret { get; init; } = string.Empty;

    [Required(ErrorMessage = "SyteLine:Username es obligatorio.")]
    public string Username { get; init; } = string.Empty;

    [Required(ErrorMessage = "SyteLine:Password es obligatorio.")]
    public string Password { get; init; } = string.Empty;

    [Required(ErrorMessage = "SyteLine:MongooseConfig es obligatorio.")]
    public string MongooseConfig { get; init; } = string.Empty;

    // ── URLs de endpoints (SENSIBLES: contienen tenant ID) ─────────────────

    [Required(ErrorMessage = "SyteLine:IdoBaseUrl es obligatorio.")]
    public string IdoBaseUrl { get; init; } = string.Empty;

    [Required(ErrorMessage = "SyteLine:PoAddItemUrl es obligatorio.")]
    public string PoAddItemUrl { get; init; } = string.Empty;

    [Required(ErrorMessage = "SyteLine:PoItemAddItemUrl es obligatorio.")]
    public string PoItemAddItemUrl { get; init; } = string.Empty;

    [Required(ErrorMessage = "SyteLine:PoItemsUrl es obligatorio.")]
    public string PoItemsUrl { get; init; } = string.Empty;

    [Required(ErrorMessage = "SyteLine:NonInventoryItemsUrl es obligatorio.")]
    public string NonInventoryItemsUrl { get; init; } = string.Empty;

    // ── Valores de negocio (NO SENSIBLES — pueden ir en appsettings.json) ──

    public string SitioDefault { get; init; } = "TECSUR";
    public string CurrCode { get; init; } = "PEN";
    public string CurrCodeDesc { get; init; } = "Nuevo Sol";
    public string TaxCode1 { get; init; } = "IGV18";
    public string TaxCode2 { get; init; } = "IGV18";
    public string TaxCode2Desc { get; init; } = "IGV 18%";
    public string TermsCode { get; init; } = "90N";
    public string TermsCodeDesc { get; init; } = "90 DÍAS";
    public string VendNum { get; init; } = "     21";
    public string VendOrder { get; init; } = string.Empty;
    public string VendorName { get; init; } = string.Empty;
    public string Whse { get; init; } = "012";
    public string NonInvAcct { get; init; } = "4212110";
}
