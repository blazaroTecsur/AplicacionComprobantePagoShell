using ComprobantePago.Application.DTOs.Comprobante.Requests;

namespace ComprobantePago.Application.Commands.Imputacion
{
    public sealed class AgregarImputacionCommand
    {
        public ImputacionDto Imputacion { get; init; } = new();
    }
}
