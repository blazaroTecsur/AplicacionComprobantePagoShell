using ComprobantePago.Application.DTOs.Comprobante.Requests;
using FluentValidation;

namespace ComprobantePago.Application.Validations
{
    public class AccionComprobanteValidator : AbstractValidator<AccionComprobanteDto>
    {
        public AccionComprobanteValidator()
        {
            RuleFor(x => x.Folio)
                .NotEmpty().WithMessage("El folio es obligatorio.")
                .MaximumLength(50).WithMessage("El folio no puede superar 50 caracteres.")
                .Matches(@"^[A-Za-z0-9\-]+$")
                    .WithMessage("El folio solo puede contener letras, números y guiones.");
        }
    }
}
