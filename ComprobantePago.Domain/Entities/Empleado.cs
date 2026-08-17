namespace ComprobantePago.Domain.Entities
{
    /// <summary>
    /// Maestro de empleados. Se sincroniza desde API externa en producción.
    /// Tabla: tmaempleado
    /// </summary>
    public class Empleado
    {
        public int IdEmpleado { get; set; }
        public string IdEmpleadoExternal { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;

        // Datos principales
        public string? Apellido { get; set; }
        public string? Nombre { get; set; }
        public string? Alias { get; set; }
        public string? Cargo { get; set; }
        public string? Departamento { get; set; }
        public string? Estado { get; set; }
        public string? Turno { get; set; }
        public string? Categoria { get; set; }
        public string? IdUsuario { get; set; }
        public string? FrecuenciaPago { get; set; }
        public string? TipoEmpleado { get; set; }
        public string? GeneraNomina { get; set; }
        public string? CuentaSueldo { get; set; }

        // Nombre tributario
        public string? PrimerNombre { get; set; }
        public string? SegundoNombre { get; set; }
        public string? PrimerApellido { get; set; }
        public string? SegundoApellido { get; set; }

        // Contacto
        public string? Direccion1 { get; set; }
        public string? Direccion2 { get; set; }
        public string? Ciudad { get; set; }
        public string? CodProvincia { get; set; }
        public string? CP { get; set; }
        public string? Telefono { get; set; }
        public string? CorreoElect { get; set; }

        // RRHH
        public DateTime? FechaContratacion { get; set; }
        public DateTime? FechaRescision { get; set; }

        public DateTime FechaCreacion { get; set; }
        public DateTime FechaActualizacion { get; set; }
    }
}
