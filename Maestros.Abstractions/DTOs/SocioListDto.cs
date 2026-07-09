namespace Maestros.Abstractions.DTOs
{
    public class SocioListDto
    {
        public long IdSocio { get; set; }
        public string CodigoSocio { get; set; } = null!;
        public string NombreCompleto { get; set; } = null!;
        public string TipoEmpleado { get; set; } = null!;
        public string? Departamento { get; set; }
        public string? Supervisor { get; set; }
        public string? CodProveedor { get; set; }
        public bool Activo { get; set; }
    }
}