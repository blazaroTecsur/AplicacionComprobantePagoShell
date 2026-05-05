using ComprobantePago.Application.DTOs.Comprobante.Requests;
using FluentValidation;

namespace ComprobantePago.Application.Validations
{
    public class BuscarComprobanteValidator : AbstractValidator<BuscarComprobanteDto>
    {
        public BuscarComprobanteValidator()
        {
            RuleFor(x => x.Proveedor)
                .MaximumLength(200).WithMessage("El proveedor no puede superar 200 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.Proveedor));

            RuleFor(x => x.Folio)
                .MaximumLength(20).WithMessage("El folio no puede superar 20 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.Folio));
        }
    }
}
