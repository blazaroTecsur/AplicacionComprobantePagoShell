using FluentValidation;
using Resguardo.Application.Exceptions;
using Resguardo.Web.Models;
using Serilog.Context;
using System.Diagnostics;

namespace Resguardo.Web.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionMiddleware(RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger,
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
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

            bool isApiRequest = context.Request.Headers["X-Requested-With"] == "XMLHttpRequest"
                             || context.Request.Path.StartsWithSegments("/api")
                             || context.Request.Headers["Accept"].ToString().Contains("application/json");

            var (statusCode, errorDetail) = ex switch
            {
                ValidationException fluentEx => (400, new ErrorDetail
                {
                    Code = "VALIDATION_ERROR",
                    UserMessage = "Por favor corrige los errores en el formulario.",
                    ValidationErrors = fluentEx.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray()
                        ),
                    TraceId = traceId
                }),

                AppException appEx => (appEx.StatusCode, new ErrorDetail
                {
                    Code = appEx.Code,
                    UserMessage = appEx.UserMessage,
                    TechnicalMessage = _env.IsDevelopment() ? appEx.Message : null,
                    TraceId = traceId
                }),

                UnauthorizedAccessException => (401, new ErrorDetail
                {
                    Code = "UNAUTHORIZED",
                    UserMessage = "No tienes permisos para realizar esta acción.",
                    TraceId = traceId
                }),

                _ => (500, new ErrorDetail
                {
                    Code = "INTERNAL_ERROR",
                    UserMessage = "Ocurrió un error inesperado. Por favor intenta de nuevo.",
                    TechnicalMessage = _env.IsDevelopment() ? ex.ToString() : null,
                    TraceId = traceId
                })
            };

            // ✅ Serilog enriquece automáticamente con contexto HTTP gracias a UseSerilog()
            using (LogContext.PushProperty("TraceId", traceId))
            using (LogContext.PushProperty("Path", context.Request.Path))
            using (LogContext.PushProperty("Method", context.Request.Method))
            {
                if (statusCode >= 500)
                    _logger.LogError(ex,
                        "Error interno | Code: {ErrorCode} | User: {User}",
                        errorDetail.Code,
                        context.User?.Identity?.Name ?? "anónimo");
                else
                    _logger.LogWarning(
                        "Error controlado | Code: {ErrorCode} | Status: {StatusCode} | User: {User}",
                        errorDetail.Code,
                        statusCode,
                        context.User?.Identity?.Name ?? "anónimo");
            }

            context.Response.StatusCode = statusCode;

            if (isApiRequest)
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(ApiResponse<object>.Fail(errorDetail));
            }
            else
            {                
                context.Response.Redirect($"/Error/Error?code={statusCode}&traceId={traceId}");
            }
        }
    }
}