using FluentValidation;

namespace Resguardo.Application.Commands.CerrarServicio
{
    public class CerrarServicioValidator : AbstractValidator<CerrarServicioCommand>
    {
        public CerrarServicioValidator()
        {
            RuleFor(x => x.IdServicioProv)
                .NotEmpty().WithMessage("El id del servicio no es válido")
                .NotNull().WithMessage("El id del servicio no es válido")
                .GreaterThan(0).WithMessage("El id del servicio no es válido");
            RuleFor(x => x.Efectivos)
                   .NotEmpty().WithMessage("Debe ingresar al menos un efectivo.")
                   .NotNull().WithMessage("Debe ingresar al menos un efectivo.");
            RuleForEach(x => x.Efectivos).ChildRules(efectivo =>
            {
                efectivo.RuleFor(x => x.IdEfectivo)
                   .NotNull().WithMessage("El id del efectivo no es válido")
                   .GreaterThan(0).WithMessage("El id del efectivo no es válido");
                efectivo.RuleFor(x => x.Asistio)                        
                        .NotNull().WithMessage("Debe marcar la asistencia.");                
            });                
        }
    }
}