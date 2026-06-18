namespace Reciclaje.Aplicacion.DTOs.Reciclaje
{
    public class ConversionarticuloDto
    {
        public int ConversionId { get; set; }
        public string ArticuloNoInventariado { get; set; } = null!;
        public string ArticuloReciclaje { get; set; } = null!;
        public string DescripcionArticuloReciclaje { get; set; }
        public DateTime? FechaCreacionAudit { get; set; }
        public string? UsuarioCreacionAudit { get; set; }
        public DateTime? FechaModificacionAudit { get; set; }
        public string? UsuarioModificacionAudit { get; set; }
    }

    public class ConversionarticuloCrearDto
    {
        public string ArticuloNoInventariado { get; set; } = null!;
        public string ArticuloReciclaje { get; set; } = null!;
        public string DescripcionArticuloReciclaje { get; set; }
    }

    public class ConversionarticuloEditarDto
    {
        public int ConversionId { get; set; }
        public string ArticuloNoInventariado { get; set; } = null!;
        public string ArticuloReciclaje { get; set; } = null!;
        public string DescripcionArticuloReciclaje { get; set; }
    }
}