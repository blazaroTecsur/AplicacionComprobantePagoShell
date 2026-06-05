namespace Maestros.Abstractions.DTOs
{
    public class ProveedorDetalleDto
    {
        public long      IdProveedor           { get; set; }
        public string    IdProveedorExternal   { get; set; } = null!;
        public string    NombreProveedor       { get; set; } = null!;
        public string    TipoPersona           { get; set; } = null!;
        public string?   Direccion1            { get; set; }
        public string?   Direccion2            { get; set; }
        public string?   Direccion3            { get; set; }
        public string?   Direccion4            { get; set; }
        public string?   Comprador             { get; set; }
        public string    Estado                { get; set; } = null!;
        public string?   Contacto              { get; set; }
        public string?   TelefonoContacto      { get; set; }
        public string?   CorreoExternoContacto { get; set; }
        public string?   CorreoInternoContacto { get; set; }
        public string    Ruc                   { get; set; } = null!;
        public DateTime  FechaReg              { get; set; }
        public DateTime? FechaAct              { get; set; }
    }
}
