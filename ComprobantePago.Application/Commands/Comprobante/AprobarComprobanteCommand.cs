using ComprobantePago.Application.DTOs.Comprobante.Requests;

namespace ComprobantePago.Application.Commands.Comprobante
{
    public sealed class AprobarComprobanteCommand
    {
        public AccionComprobanteDto Comprobante { get; init; } = new();
    }
}
