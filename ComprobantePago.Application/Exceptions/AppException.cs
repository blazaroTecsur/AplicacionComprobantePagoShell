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
    }
}
