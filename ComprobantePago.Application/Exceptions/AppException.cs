namespace ComprobantePago.Application.Exceptions
{
    /// <summary>
    /// Excepción base de la aplicación. Permite definir un código semántico,
    /// mensaje amigable para el usuario y código HTTP de respuesta.
    /// </summary>
    public class AppException : Exception
    {
        public int    StatusCode   { get; }
        public string Code         { get; }
        public string UserMessage  { get; }

        public AppException(
            string userMessage,
            string code       = "APP_ERROR",
            int    statusCode = 400)
            : base(userMessage)
        {
            UserMessage  = userMessage;
            Code         = code;
            StatusCode   = statusCode;
        }
        public class NotFoundException : AppException
        {
            public NotFoundException(string resource)
                : base("NOT_FOUND", $"El recurso '{resource}' no fue encontrado.", 404) { }
        }
        public class BusinessException : AppException
        {
            public BusinessException(string code, string userMessage)
                : base(code, userMessage, 400) { }
        }
        public class UnauthorizedException : AppException
        {
            public UnauthorizedException()
                : base("UNAUTHORIZED", "No tienes permisos para realizar esta acción.", 401) { }
        }
    }
}
