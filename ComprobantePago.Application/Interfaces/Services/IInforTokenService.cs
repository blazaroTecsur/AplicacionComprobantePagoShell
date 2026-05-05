namespace ComprobantePago.Application.Interfaces.Services
{
    /// <summary>
    /// Gestiona la obtención y caché del token OAuth2 para la API de Infor ION.
    /// </summary>
    public interface IInforTokenService
    {
        /// <summary>
        /// Devuelve un access_token válido, reutilizando el cacheado si aún no venció.
        /// </summary>
        Task<string> ObtenerTokenAsync();
    }
}
