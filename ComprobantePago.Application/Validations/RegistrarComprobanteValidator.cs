using ComprobantePago.Application.DTOs.Comprobante.Requests;
using FluentValidation;

namespace ComprobantePago.Application.Validations
{
    public class RegistrarComprobanteValidator : AbstractValidator<RegistrarComprobanteDto>
    {
        public RegistrarComprobanteValidator()
        {
            RuleFor(x => x.Ruc)
                .NotEmpty().WithMessage("El RUC es obligatorio.")
                .Length(11).WithMessage("El RUC debe tener exactamente 11 dígitos.")
                .Matches(@"^\d+$").WithMessage("El RUC solo debe contener dígitos.");

            RuleFor(x => x.RazonSocial)
                .NotEmpty().WithMessage("La razón social es obligatoria.")
                .MaximumLength(200).WithMessage("La razón social no puede superar 200 caracteres.");

            RuleFor(x => x.TipoDocumento)
                .NotEmpty().WithMessage("El tipo de documento es obligatorio.");

            RuleFor(x => x.TipoSunat)
                .NotEmpty().WithMessage("El tipo SUNAT es obligatorio.");

            RuleFor(x => x.Serie)
                .NotEmpty().WithMessage("La serie es obligatoria.")
                .MaximumLength(10).WithMessage("La serie no puede superar 10 caracteres.");

            RuleFor(x => x.Numero)
                .NotEmpty().WithMessage("El número de comprobante es obligatorio.")
                .MaximumLength(20).WithMessage("El número no puede superar 20 caracteres.");

            RuleFor(x => x.FechaEmision)
                .NotEmpty().WithMessage("La fecha de emisión es obligatoria.");

            RuleFor(x => x.Moneda)
                .NotEmpty().WithMessage("La moneda es obligatoria.");

            RuleFor(x => x.TasaCambio)
                .GreaterThanOrEqualTo(0).WithMessage("La tasa de cambio no puede ser negativa.");

            RuleFor(x => x.MontoTotal)
                .GreaterThan(0).WithMessage("El monto total debe ser mayor a 0.");

            RuleFor(x => x.MontoNeto)
                .GreaterThanOrEqualTo(0).WithMessage("El monto neto no puede ser negativo.");

            RuleFor(x => x.MontoIGVCredito)
                .GreaterThanOrEqualTo(0).WithMessage("El monto IGV no puede ser negativo.");

            RuleFor(x => x.TipoDetraccion)
                .NotEmpty().WithMessage("El tipo de detracción es obligatorio cuando aplica detracción.")
                .When(x => x.TieneDetraccion);

            RuleFor(x => x.PorcentajeDetraccion)
                .InclusiveBetween(0, 100)
                .WithMessage("El porcentaje de detracción debe estar entre 0 y 100.")
                .When(x => x.TieneDetraccion);

            RuleFor(x => x.MontoDetraccion)
                .GreaterThanOrEqualTo(0).WithMessage("El monto de detracción no puede ser negativo.")
                .When(x => x.TieneDetraccion);
        }
    }
}
