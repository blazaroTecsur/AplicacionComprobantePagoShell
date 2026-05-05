using ComprobantePago.Application.DTOs.Comprobante.Requests;

namespace ComprobantePago.Application.Commands.Comprobante
{
    public sealed class DerivarComprobanteCommand
    {
        public AccionComprobanteDto Comprobante { get; init; } = new();
        public int? VoucherSyteline { get; init; }
    }
}
