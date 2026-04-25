using FluentValidation;

namespace Resguardo.Application.Commands.RegistrarSolicitud
{
    public class RegistrarSolicitudValidator : AbstractValidator<RegistrarSolicitudCommand>
    {
        public RegistrarSolicitudValidator()
        {
            RuleFor(x => x.IdFlujo)
                   .NotNull().WithMessage("Debe seleccionar el flujo")
                   .GreaterThan(0).WithMessage("Debe seleccionar el flujo");
            RuleFor(x => x.NumSro)
                    .NotEmpty().WithMessage("Debe ingresar el número de SRO.")
                    .MaximumLength(10).WithMessage("La longitud máxima del número de la SRO es de 10 caracteres.");
            RuleFor(x => x.CodCapataz)
                    .NotEmpty().WithMessage("Debe ingresar el capataz.")
                    .MaximumLength(15).WithMessage("La longitud máxima del capataz es de 15 caracteres.");
            RuleFor(x => x.NomCapataz)
                    .NotEmpty().WithMessage("Debe ingresar el capataz.")
                    .MaximumLength(50).WithMessage("La longitud máxima del capataz es de 50 caracteres.");
            RuleFor(x => x.Celular)
                    .NotEmpty().WithMessage("Debe ingresar el celular.")
                    .MaximumLength(15).WithMessage("La longitud máxima del celular es de 15 caracteres.");
            RuleFor(x => x.TpoTrabajo)
                    .NotEmpty().WithMessage("Debe ingresar el tipo de trabajo.")
                    .MaximumLength(200).WithMessage("La longitud máxima del tipo de trabajo es de 200 caracteres.");
            RuleFor(x => x.Servicios)
                    .NotEmpty().WithMessage("Debe ingresar al menos un servicio.")
                    .NotNull().WithMessage("Debe ingresar al menos un servicio.");
            RuleForEach(x => x.Servicios).ChildRules(servicio =>
            {
                servicio.RuleFor(x => x.IdTpoServicio)
                .NotNull().WithMessage("Debe seleccionar el tipo")
                .GreaterThan(0).WithMessage("Debe seleccionar el tipo");
                servicio.RuleFor(x => x.Fecha)
                    .NotNull().WithMessage("Debe ingresar la fecha")
                    .NotEmpty().WithMessage("Debe ingresar la fecha");
                servicio.RuleFor(x => x.HraInicio)
                    .NotEmpty().WithMessage("Debe ingresar la hora de inicio")
                    .Length(5).WithMessage("El formato de la hora no es correcto.");
                servicio.RuleFor(x => x.HraFinal)
                    .NotEmpty().WithMessage("Debe ingresar la hora final")
                    .Length(5).WithMessage("El formato de la hora no es correcto.");
                servicio.RuleFor(x => x.Coordenada)
                    .NotEmpty().WithMessage("Debe ingresar la coordenada")
                    .MaximumLength(50).WithMessage("La longitud máxima de la coordenada es de 50 caracteres.");
                servicio.RuleFor(x => x.Direccion)
                    .NotEmpty().WithMessage("Debe ingresar la dirección")
                    .MaximumLength(200).WithMessage("La longitud máxima de la dirección es de 200 caracteres.");
                servicio.RuleFor(x => x.Cantidad)
                    .NotNull().WithMessage("Debe ingresar la cantidad")
                    .NotEmpty().WithMessage("Debe ingresar la cantidad");
            });
        }
    }
}