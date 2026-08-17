using Serilog.Context;
using System.Security.Claims;

namespace ComprobantePago.Web.Middlewares
{
    /// <summary>
    /// Middleware que garantiza que cada petición HTTP tenga:
    ///   - CorrelationId: leído de X-Correlation-Id o generado como GUID nuevo.
    ///   - RequestId    : TraceIdentifier de ASP.NET Core.
    ///   - UserId       : correo/upn del usuario autenticado (o "anónimo").
    /// Todos se inyectan en el LogContext de Serilog para que aparezcan
    /// en todos los logs emitidos durante la petición.
    /// </summary>
    public class CorrelationIdMiddleware
    {
        private const string CorrelationHeader = "X-Correlation-Id";
        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            // 1. CorrelationId: reutiliza el header entrante o genera uno nuevo
            var correlationId = context.Request.Headers[CorrelationHeader].FirstOrDefault()
                ?? Guid.NewGuid().ToString("N");

            // Devolver el mismo ID en la respuesta para trazabilidad cliente-servidor
            context.Response.Headers[CorrelationHeader] = correlationId;

            // 2. RequestId: identificador interno de ASP.NET Core
            var requestId = context.TraceIdentifier;

            // 3. UserId: preferred_username → upn → email → Name → "anónimo"
            var userId = ObtenerUserId(context.User);

            using (LogContext.PushProperty("CorrelationId", correlationId))
            using (LogContext.PushProperty("RequestId",     requestId))
            using (LogContext.PushProperty("UserId",        userId))
            {
                await _next(context);
            }
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
