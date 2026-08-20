using Seguridad.Abstractions.Interfaces;

namespace ComprobantePago.Infrastructure.Extensions
{
    public static class UsuarioContextoExtensions
    {
        /// <summary>
        /// Devuelve el código de empresa sin el prefijo "S".
        /// Sitio = "STECSUR" → CodigoEmpresa = "TECSUR"
        /// </summary>
        public static string CodigoEmpresa(this IUsuarioContexto usuario)
        {
            var sitio = usuario.Sitio;
            if (string.IsNullOrEmpty(sitio)) return string.Empty;
            return sitio.StartsWith("S", StringComparison.OrdinalIgnoreCase)
                ? sitio[1..]
                : sitio;
        }
    }
}
