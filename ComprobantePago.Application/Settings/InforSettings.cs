namespace ComprobantePago.Application.Settings
{
    /// <summary>
    /// Configuración para la API IDO REST de Infor CloudSuite / Syteline.
    /// Los nombres de propiedad corresponden directamente a los campos del archivo .ionapi.
    /// </summary>
    public class InforSettings
    {
        public const string Section = "Infor";

        /// <summary>URL base del portal ION API — campo iu del .ionapi (mingle-ionapi)</summary>
        public string BaseUrl { get; set; } = string.Empty;

        /// <summary>
        /// Dominio del SSO de Infor — campo pu del .ionapi solo con el host.
        /// Ej: https://mingle-sso.inforcloudsuite.com:443
        /// El tenant se toma automáticamente del campo Tenant.
        /// </summary>
        public string SsoBaseUrl { get; set; } = string.Empty;

        /// <summary>Tenant ID — campo ti del .ionapi</summary>
        public string Tenant { get; set; } = string.Empty;

        /// <summary>Identificador lógico de la aplicación Syteline (ej. CSI)</summary>
        public string AppId { get; set; } = string.Empty;

        /// <summary>OAuth2 Client ID — campo ci del .ionapi</summary>
        public string ClientId { get; set; } = string.Empty;

        /// <summary>OAuth2 Client Secret — campo cs del .ionapi</summary>
        public string ClientSecret { get; set; } = string.Empty;

        /// <summary>Service Account API Key — campo saak del .ionapi (parte del username).</summary>
        public string ServiceAccountKey { get; set; } = string.Empty;

        /// <summary>Service Account Secret Key — campo sask del .ionapi (password).</summary>
        public string ServiceAccountSecret { get; set; } = string.Empty;

        // ── URLs derivadas ────────────────────────────────────────────────────

        /// <summary>
        /// Token endpoint construido como: {SsoBaseUrl}/{Tenant}/as/token.oauth2
        /// Equivale exactamente a pu + ot del .ionapi.
        /// </summary>
        public string TokenEndpoint =>
            $"{SsoBaseUrl.TrimEnd('/')}/{Tenant}/as/token.oauth2";

        /// <summary>
        /// Nombre de la configuración Mongoose de Syteline.
        /// Se envía en el header IFS-SL-Config en cada request al IDO REST.
        /// Ejemplo: "SL_Production", "TECSUR"
        /// </summary>
        public string Configuration { get; set; } = string.Empty;

        /// <summary>
        /// Site de Syteline destino para los comprobantes (campo UbToSite en SLAptrxs).
        /// Ejemplo: "LIMA", "SITE1"
        /// </summary>
        public string Site { get; set; } = string.Empty;

        /// <summary>URL base del servicio IDO REST</summary>
        public string IdoBaseUrl =>
            $"{BaseUrl.TrimEnd('/')}/{Tenant}/{AppId}/IDORequestService/MGRestService.svc/";
    }
}
