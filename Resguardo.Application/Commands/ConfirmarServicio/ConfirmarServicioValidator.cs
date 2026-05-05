using FluentValidation;

namespace Resguardo.Application.Commands.ConfirmarServicio
{
    public class ConfirmarServicioValidator : AbstractValidator<ConfirmarServicioCommand>
    {
        public ConfirmarServicioValidator()
        {
            RuleFor(x => x.IdSolicitud)
                   .NotNull().WithMessage("El id de la solicitud no es válido")
                   .GreaterThan(0).WithMessage("El id de la solicitud no es válido");
            RuleFor(x => x.Servicios)
                    .NotEmpty().WithMessage("Debe ingresar al menos un servicio.")
                    .NotNull().WithMessage("Debe ingresar al menos un servicio.");
            RuleForEach(x => x.Servicios).ChildRules(servicio =>
            {                
                servicio.RuleFor(x => x.IdServicio)
                .NotNull().WithMessage("El id del servicio no es válido")
                .GreaterThan(0).WithMessage("El id del servicio no es válido");
                servicio.RuleFor(x => x.IdProveedor)
                .NotNull().WithMessage("Debe ingresar el dato del proveedor")
                .GreaterThan(0).WithMessage("Debe ingresar el dato del proveedor");
                servicio.RuleFor(x => x.Cantidad)
                .NotNull().WithMessage("Debe ingresar la cantidad")
                .GreaterThan(0).WithMessage("Debe ingresar la cantidad");
            });
        }
    }
}