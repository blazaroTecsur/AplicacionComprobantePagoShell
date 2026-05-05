using FluentValidation;

namespace Resguardo.Application.Commands.RegistrarConfig
{
    public class RegistrarConfigValidator : AbstractValidator<RegistrarConfigCommand>
    {
        public RegistrarConfigValidator()
        {
            RuleFor(x => x.Fecha)
                .NotNull().WithMessage("Debe ingresar fecha")
                .Must(fecha => fecha >= DateOnly.FromDateTime(DateTime.Now.Date)).WithMessage("La fecha debe ser igual o mayor a la actual");
            RuleForEach(x => x.Configs).ChildRules(config =>
                        {                            
                            config.RuleFor(x => x.CodDpto)
                .NotEmpty().WithMessage("Debe ingresar el departamento")
                .NotNull().WithMessage("Debe ingresar el departamento");
                            config.RuleFor(x => x.IdTpoServicio)
                .NotNull().WithMessage("Debe ingresar el tipo de servicio")
                .GreaterThan(0).WithMessage("Debe ingresar el tipo de servicio");
                            config.RuleFor(x => x.Diurno)
                .NotNull().WithMessage("Debe ingresar la cantidad diurna")
                .GreaterThanOrEqualTo(0).WithMessage("Debe ingresar la cantidad diurna");
                            config.RuleFor(x => x.Nocturno)
                .NotNull().WithMessage("Debe ingresar la cantidad nocturna")
                .GreaterThanOrEqualTo(0).WithMessage("Debe ingresar la cantidad nocturna");
                        });
        }
    }
}