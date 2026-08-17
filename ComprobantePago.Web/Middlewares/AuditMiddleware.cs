using Serilog;
using System.Security.Claims;
using ILogger = Serilog.ILogger;

namespace ComprobantePago.Web.Middlewares
{
    /// <summary>
    /// Middleware de auditoría.
    /// Intercepta POST a endpoints críticos y registra, DESPUÉS de que la respuesta
    /// es enviada, un evento de auditoría estructurado via Serilog con la
    /// propiedad <c>AuditLog = true</c> (usada para filtrar al sink dedicado).
    ///
    /// Acciones auditadas:
    ///   ALTA_MODIFICACION  — /Comprobante/Guardar
    ///   ENVIO              — /Comprobante/Enviar
    ///   AUTORIZACION       — /Comprobante/Firmar
    ///   APROBACION         — /Comprobante/Aprobar
    ///   ANULACION          — /Comprobante/Anular
    ///   DERIVACION         — /Comprobante/Derivar
    ///   ALTA_IMPUTACION    — /Comprobante/AgregarImputacion
    ///   MOD_IMPUTACION     — /Comprobante/EditarImputacion
    ///   BAJA_IMPUTACION    — /Comprobante/EliminarImputacion
    ///   CARGA_MASIVA       — /Comprobante/CargarImputacionMasiva
    ///   ALTA_DOCUMENTO     — /Comprobante/SubirDocumentos
    ///   BAJA_DOCUMENTO     — /Comprobante/EliminarDocumento
    ///   EXPORTACION        — /Comprobante/ExportarDistribucionSyteline
    ///   EXPORTACION        — /Comprobante/ExportarCabeceraSyteline
    /// </summary>
    public class AuditMiddleware
    {
        private static readonly Dictionary<string, string> _acciones =
            new(StringComparer.OrdinalIgnoreCase)
        {
            ["/Comprobante/Guardar"]                    = "ALTA_MODIFICACION",
            ["/Comprobante/Enviar"]                     = "ENVIO",
            ["/Comprobante/Firmar"]                     = "AUTORIZACION",
            ["/Comprobante/Aprobar"]                    = "APROBACION",
            ["/Comprobante/Anular"]                     = "ANULACION",
            ["/Comprobante/Derivar"]                    = "DERIVACION",
            ["/Comprobante/AgregarImputacion"]          = "ALTA_IMPUTACION",
            ["/Comprobante/EditarImputacion"]           = "MOD_IMPUTACION",
            ["/Comprobante/EliminarImputacion"]         = "BAJA_IMPUTACION",
            ["/Comprobante/CargarImputacionMasiva"]     = "CARGA_MASIVA",
            ["/Comprobante/SubirDocumentos"]            = "ALTA_DOCUMENTO",
            ["/Comprobante/EliminarDocumento"]          = "BAJA_DOCUMENTO",
            ["/Comprobante/ExportarDistribucionSyteline"] = "EXPORTACION",
            ["/Comprobante/ExportarCabeceraSyteline"]  = "EXPORTACION",
        };

        // Logger con la propiedad AuditLog fija → se enruta al sink de auditoría
        private static readonly ILogger _audit =
            Log.ForContext("AuditLog", true);

        private readonly RequestDelegate _next;

        public AuditMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? "";

            // Solo interceptar POST a rutas críticas
            if (!context.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase)
                || !_acciones.TryGetValue(path, out var accion))
            {
                await _next(context);
                return;
            }

            var inicio = DateTime.UtcNow;
            await _next(context);
            var duracionMs = (DateTime.UtcNow - inicio).TotalMilliseconds;

            var userId = ObtenerUserId(context.User);
            var http   = context.Response.StatusCode;
            var exito  = http is >= 200 and < 300;

            _audit.Information(
                "[AUDIT] {Accion} | {UserId} | {Path} | HTTP {StatusCode} | {DuracionMs:F0}ms | {Resultado}",
                accion, userId, path, http, duracionMs,
                exito ? "OK" : "FALLO");
        }

        private static string ObtenerUserId(ClaimsPrincipal? usuario)
        {
            if (usuario is null) return "anónimo";

            string[] tipos =
            [
                "preferred_username",
                "upn",
                "email",
                ClaimTypes.Email,
                ClaimTypes.Upn,
                ClaimTypes.Name
            ];

            foreach (var tipo in tipos)
            {
                var valor = usuario.FindFirst(tipo)?.Value;
                if (!string.IsNullOrWhiteSpace(valor)) return valor;
            }

            return usuario.Identity?.Name ?? "anónimo";
        }
    }
}
