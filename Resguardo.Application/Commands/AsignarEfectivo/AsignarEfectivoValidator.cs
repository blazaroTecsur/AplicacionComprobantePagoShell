using FluentValidation;

namespace Resguardo.Application.Commands.AsignarEfectivo
{
    public class AsignarEfectivoValidator : AbstractValidator<AsignarEfectivoCommand>
    {
        public AsignarEfectivoValidator()
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
                efectivo.RuleFor(x => x.Dni)
                .NotEmpty().WithMessage("Debe ingresar el DNI")
                .NotNull().WithMessage("Debe ingresar el DNI")
                .MaximumLength(15).WithMessage("La longitud máxima del DNI es de 15 caracteres.");
                efectivo.RuleFor(x => x.Nombres)
                    .NotEmpty().WithMessage("Debe ingresar el nombre")
                    .NotNull().WithMessage("Debe ingresar el nombre")
                    .MaximumLength(50).WithMessage("La longitud máxima del nombre es de 50 caracteres.");
                efectivo.RuleFor(x => x.Apellidos)
                    .NotEmpty().WithMessage("Debe ingresar el apellido")
                    .NotNull().WithMessage("Debe ingresar el apellido")
                    .MaximumLength(50).WithMessage("La longitud máxima del apellido es de 50 caracteres.");
                efectivo.RuleFor(x => x.Telefono)
                    .NotEmpty().WithMessage("El # de teléfono no es válido")
                    .NotNull().WithMessage("El # de teléfono no es válido")
                    .MaximumLength(15).WithMessage("La longitud máxima del celular es de 15 caracteres.");
            });
        }
    }
}