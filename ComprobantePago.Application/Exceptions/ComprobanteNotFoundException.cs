namespace ComprobantePago.Application.Exceptions
{
    public class ComprobanteNotFoundException : AppException
    {
        public ComprobanteNotFoundException(string folio)
            : base(
                userMessage: $"No se encontró el comprobante con folio '{folio}'.",
                code: "COMPROBANTE_NOT_FOUND",
                statusCode: 404)
        { }
    }
}
