using ComprobantePago.Application.Common;
using ComprobantePago.Application.Exceptions;
using FluentValidation;
using Serilog.Context;
using System.Diagnostics;

namespace ComprobantePago.Web.Middlewares
{
    /// <summary>
    /// Manejo centralizado de excepciones.
    /// Las solicitudes AJAX/API reciben BaseResponse con HTTP 200 para compatibilidad con cliente.
    /// Las solicitudes de navegador son redirigidas a la página de error.
    /// </summary>
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(
            RequestDelegate next,
            ILogger<ExceptionMiddleware> logger,
            IHostEnvironment env)
        {
            _next   = next;
            _logger = logger;
            _env    = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

            bool isAjaxRequest =
                context.Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                context.Request.Path.StartsWithSegments("/api")                  ||
                context.Request.Headers["Accept"].ToString().Contains("application/json");

            var (statusCode, mensajeUsuario) = ex switch
            {
                ValidationException fluentEx =>
                    (400, string.Join("; ", fluentEx.Errors.Select(e => e.ErrorMessage))),

                AppException appEx =>
                    (appEx.StatusCode, appEx.UserMessage),

                UnauthorizedAccessException =>
                    (401, "No tienes permisos para realizar esta acción."),

                _ =>
                    (500, "Ocurrió un error inesperado. Por favor intenta de nuevo.")
            };

            using (LogContext.PushProperty("TraceId", traceId))
            using (LogContext.PushProperty("Path",    context.Request.Path))
            using (LogContext.PushProperty("Method",  context.Request.Method))
            {
                if (statusCode >= 500)
                    _logger.LogError(ex,
                        "Error interno [{TraceId}] | Usuario: {User}",
                        traceId,
                        context.User?.Identity?.Name ?? "anónimo");
                else
                    _logger.LogWarning(
                        "Error controlado [{TraceId}] | HTTP {StatusCode} | {Message}",
                        traceId, statusCode, ex.Message);
            }

            if (isAjaxRequest)
            {
                // Devuelve siempre HTTP 200 con BaseResponse para compatibilidad con cliente JS
                context.Response.StatusCode  = 200;
                context.Response.ContentType = "application/json";

                var mensaje = _env.IsDevelopment()
                    ? $"{mensajeUsuario} [TraceId: {traceId}]"
                    : mensajeUsuario;

                await context.Response.WriteAsJsonAsync(
                    BaseResponse.Error(mensaje));
            }
            else
            {
                context.Response.Redirect(
                    $"/Home/Error?code={statusCode}&traceId={traceId}");
            }
        }
    }
}
