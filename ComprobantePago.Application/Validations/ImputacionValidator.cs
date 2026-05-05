using ComprobantePago.Application.DTOs.Comprobante.Requests;
using FluentValidation;

namespace ComprobantePago.Application.Validations
{
    public class ImputacionValidator : AbstractValidator<ImputacionDto>
    {
        public ImputacionValidator()
        {
            RuleFor(x => x.Folio)
                .NotEmpty().WithMessage("El folio es obligatorio.");

            RuleFor(x => x.CuentaContable)
                .NotEmpty().WithMessage("La cuenta contable es obligatoria.");

            RuleFor(x => x.DescripcionCuenta)
                .NotEmpty().WithMessage("La descripción de cuenta es obligatoria.")
                .MaximumLength(200).WithMessage("La descripción no puede superar 200 caracteres.");

            RuleFor(x => x)
                .Must(x => !string.IsNullOrWhiteSpace(x.CodUnidad1Cuenta)
                        || !string.IsNullOrWhiteSpace(x.CodUnidad3Cuenta)
                        || !string.IsNullOrWhiteSpace(x.CodUnidad4Cuenta))
                .WithMessage("Debe indicar al menos un código de unidad (1, 3 o 4).")
                .WithName("CodUnidad");
        }
    }
}
