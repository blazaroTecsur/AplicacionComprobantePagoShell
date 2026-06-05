namespace Maestros.Abstractions.DTOs
{
    public class ProveedorListDto
    {
        public long    IdProveedor         { get; set; }
        public string  IdProveedorExternal { get; set; } = null!;
        public string  NombreProveedor     { get; set; } = null!;
        public string  Ruc                 { get; set; } = null!;
        public string  TipoPersona         { get; set; } = null!;
        public string  Estado              { get; set; } = null!;
        public string? Comprador           { get; set; }
    }
}
