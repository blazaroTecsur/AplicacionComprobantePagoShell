using FluentValidation;

namespace Resguardo.Application.DTOs.Infor
{
    public class ObtenerOrdenValidator : AbstractValidator<ObtenerOrdenResponse>
    {
        public ObtenerOrdenValidator()
        {
            RuleFor(x => x.Id)
                   .NotNull().NotEmpty().WithMessage("El ID de la orden no puede ser vacío.");
            RuleFor(x => x.NumSro)
                .NotNull().NotEmpty().WithMessage("El número de la orden no puede ser vacío.");
            RuleFor(x => x.Descripcion)
                .NotNull().NotEmpty().WithMessage("La descripción de la orden no puede ser vacío.");
            RuleFor(x => x.CodDpto)
                .NotNull().NotEmpty().WithMessage("El código de departamento de la orden no puede ser vacío.");
            RuleFor(x => x.NomDpto)
                .NotNull().NotEmpty().WithMessage("El nombre de departamento de la orden no puede ser vacío.");
            RuleFor(x => x.CodActv)
                .NotNull().NotEmpty().WithMessage("El código de actividad de la orden no puede ser vacío.");
            RuleFor(x => x.NomActv)
                .NotNull().NotEmpty().WithMessage("El nombre de la actividad de la orden no puede ser vacío.");
            RuleFor(x => x.CodSctta)
                .NotNull().NotEmpty().WithMessage("El código de subcontratista de la orden no puede ser vacío.");
            RuleFor(x => x.NomSctta)
                .NotNull().NotEmpty().WithMessage("El nombre de subcontratista de la orden no puede ser vacío.");
            RuleFor(x => x.FechaFoc)
                .NotNull().NotEmpty().WithMessage("La fecha foc de la orden no puede ser vacío.");
            RuleFor(x => x.Estado)
                .NotNull().NotEmpty().WithMessage("El estado de la orden no puede ser vacío.");
            RuleFor(x => x.Coordenada)
                .NotNull().NotEmpty().WithMessage("La coordenada de la orden no puede ser vacío.");
            RuleFor(x => x.Direccion)
                .NotNull().NotEmpty().WithMessage("La dirección de la orden no puede ser vacío.");
        }
    }
}