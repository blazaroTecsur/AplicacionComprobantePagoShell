using ComprobantePago.Application.DTOs.Comprobante.Requests;

namespace ComprobantePago.Application.Commands.Imputacion
{
    public sealed class EditarImputacionCommand
    {
        public ImputacionDto Imputacion { get; init; } = new();
    }
}
