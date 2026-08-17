namespace ComprobantePago.Application.Exceptions
{
    public class ImputacionNotFoundException : AppException
    {
        public ImputacionNotFoundException(string folio, int secuencia)
            : base(
                userMessage: $"No se encontró la imputación {secuencia} para el folio '{folio}'.",
                code: "IMPUTACION_NOT_FOUND",
                statusCode: 404)
        { }
    }
}
