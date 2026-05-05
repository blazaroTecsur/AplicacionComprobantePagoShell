using FluentValidation;

namespace Resguardo.Application.Commands.EditarSolicitud
{
    public class EditarSolicitudValidator : AbstractValidator<EditarSolicitudCommand>
    {
        public EditarSolicitudValidator()
        {            
            RuleFor(x => x.CodCapataz)
                    .NotEmpty().WithMessage("Debe ingresar el capataz.")
                    .MaximumLength(15).WithMessage("La longitud máxima del capataz es de 15 caracteres.");
            RuleFor(x => x.NomCapataz)
                    .NotEmpty().WithMessage("Debe ingresar el capataz.")
                    .MaximumLength(50).WithMessage("La longitud máxima del capataz es de 50 caracteres.");
            RuleFor(x => x.Celular)
                    .NotEmpty().WithMessage("Debe ingresar el celular.")
                    .MaximumLength(15).WithMessage("La longitud máxima del celular es de 15 caracteres.");            
            RuleFor(x => x.Servicios)
                    .NotEmpty().WithMessage("Debe ingresar al menos un servicio.")
                    .NotNull().WithMessage("Debe ingresar al menos un servicio.");
            RuleForEach(x => x.Servicios).ChildRules(servicio =>
            {                
                servicio.RuleFor(x => x.HraInicio)
                    .NotEmpty().WithMessage("Debe ingresar la hora de inicio")
                    .Length(5).WithMessage("El formato de la hora no es correcto.");
                servicio.RuleFor(x => x.HraFinal)
                    .NotEmpty().WithMessage("Debe ingresar la hora final")
                    .Length(5).WithMessage("El formato de la hora no es correcto.");             
            });
        }
    }
}