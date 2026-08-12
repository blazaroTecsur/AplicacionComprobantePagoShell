using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Reciclaje.Dominio.Interfaces;
using System.Net;

namespace Reciclaje.Aplicacion.Filtros
{
    public class ExcepcionGlobalFiltro : IAsyncExceptionFilter
    {
        private readonly ILogger<ExcepcionGlobalFiltro> _logger;
        //private readonly IUnidadTrabajo _unidadTrabajo;
        public class ExcepcionGlobal
        {
            public int Estado { get; set; }
            public string Titulo { get; set; }
            public string Mensaje { get; set; }
        }
        public ExcepcionGlobalFiltro(ILogger<ExcepcionGlobalFiltro> logger)
        {
            _logger = logger;
            //_unidadTrabajo = unidadTrabajo;
        }
        public async Task OnExceptionAsync(ExceptionContext context)
        {
            if (!context.ExceptionHandled)
            {
                //await _unidadTrabajo.RollbackTransactionAsync();

                _logger.LogError(context.Exception, context.Exception.Message);

                var excepcion = new ExcepcionGlobal
                {
                    Estado = (int)HttpStatusCode.BadRequest,
                    Titulo = "Solicitudes incorrecta",
                    Mensaje = "Su solicitud no pudo ser procesada debido a un error en los datos enviados. Por favor, revise los campos e intente nuevamente."
                };
                if (context.Exception is UnauthorizedAccessException)
                {
                    excepcion.Estado = (int)HttpStatusCode.Unauthorized;
                    excepcion.Titulo = "No autorizado";
                    excepcion.Mensaje = "No tiene autorización para acceder a este recurso. Por favor, inicie sesión con sus credenciales y vuelva a intentarlo.";
                }
                else if (context.Exception is FileNotFoundException)
                {
                    excepcion.Estado = (int)HttpStatusCode.NotFound;
                    excepcion.Titulo = "No encontrado";
                    excepcion.Mensaje = "El recurso que está buscando no fue encontrado. Verifique la URL o póngase en contacto con el soporte si cree que esto es un error.";
                }
                else if (context.Exception is InvalidOperationException excepcionExcepcion)
                {
                    excepcion.Estado = (int)HttpStatusCode.NotAcceptable;
                    excepcion.Titulo = "No aceptable";
                    excepcion.Mensaje = excepcionExcepcion.Message;
                }

                var json = new
                {
                    error = excepcion
                };

                context.Result = new BadRequestObjectResult(json);
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.ExceptionHandled = true;

                var mensajeError = $@"  <h3>Error en la aplicación</h3><p><strong>Mensaje:</strong> {context.Exception.Message}</p>
                                        <p><strong>StackTrace:</strong><pre>{context.Exception.StackTrace}</pre></p>
                                        <p><strong>Ruta:</strong> {context.HttpContext.Request.Path}</p>
                                        <p><strong>Fecha:</strong> {DateTime.UtcNow}</p>";
                
            }
        }
    }
}