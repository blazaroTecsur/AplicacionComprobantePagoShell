using FluentValidation;

namespace Resguardo.Application.Commands.AprobarSolicitud
{
    public class AprobarSolicitudValidator : AbstractValidator<AprobarSolicitudCommand>
    {
        public AprobarSolicitudValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("El id de la solicitud no es válido")
                .NotNull().WithMessage("El id de la solicitud no es válido")
                .GreaterThan(0).WithMessage("El id de la solicitud no es válido");
            RuleFor(x => x.CodEstado)
                .NotEmpty().WithMessage("Debe ingresar el código de estado")
                .NotNull().WithMessage("Debe ingresar el código de estado");
            RuleFor(x => x.Servicios)
                   .NotEmpty().WithMessage("Debe ingresar al menos un servicio.")
                   .NotNull().WithMessage("Debe ingresar al menos un servicio.");
            RuleForEach(x => x.Servicios).ChildRules(servicio =>
            {
                servicio.RuleFor(x => x.Id)
                .NotEmpty().WithMessage("El id del servicio no es válido")
                .NotNull().WithMessage("El id del servicio no es válido")
                .GreaterThan(0).WithMessage("El id del servicio no es válido");
                servicio.RuleFor(x => x.Cantidad)
                    .NotEmpty().WithMessage("Debe ingresar la cantidad")
                    .NotNull().WithMessage("Debe ingresar la cantidad");
            });
        }
    }
}
