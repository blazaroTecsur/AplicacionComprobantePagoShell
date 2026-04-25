namespace Resguardo.Application.Exceptions
{
    public abstract class AppException : Exception
    {
        public string Code { get; }
        public string UserMessage { get; }
        public int StatusCode { get; }

        protected AppException(string code, string userMessage,
            int statusCode = 400, string? technicalMessage = null)
            : base(technicalMessage ?? userMessage)
        {
            Code = code;
            UserMessage = userMessage;
            StatusCode = statusCode;
        }
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
