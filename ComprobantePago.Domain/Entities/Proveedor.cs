namespace ComprobantePago.Domain.Entities
{
    /// <summary>
    /// Maestro de proveedores. Tabla: tmaproveedor
    /// </summary>
    public class Proveedor
    {
        public int IdProveedor { get; set; }
        public int IdProveedorExternal { get; set; }
        public string? NombreProveedor { get; set; }
        public string TipoPersona { get; set; } = string.Empty;
        public string? Direccion1 { get; set; }
        public string? Direccion2 { get; set; }
        public string? Direccion3 { get; set; }
        public string? Direccion4 { get; set; }
        public string? Comprador { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string? Contacto { get; set; }
        public string? TelefonoContacto { get; set; }
        public string? CorreoExternoContacto { get; set; }
        public string? CorreoInternoContacto { get; set; }
        public string Ruc { get; set; } = string.Empty;
        public string? UsuarioReg { get; set; }
        public DateTime FechaReg { get; set; }
        public string? UsuarioAct { get; set; }
        public DateTime? FechaAct { get; set; }
    }
}
