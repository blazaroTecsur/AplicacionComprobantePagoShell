using ComprobantePago.Application.DTOs.Comprobante.Requests;

namespace ComprobantePago.Application.Commands.Comprobante
{
    public sealed class RegistrarComprobanteCommand
    {
        public RegistrarComprobanteDto Comprobante { get; init; } = new();
    }
}
