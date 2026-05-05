namespace ComprobantePago.Domain.Entities
{
    public class TipoDetraccion
    {
        public int IdTipoDetraccion { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Porcentaje { get; set; }
        public bool Activo { get; set; } = true;
        public string UsuarioReg { get; set; } = string.Empty;
        public DateTime FechaReg { get; set; } = DateTime.Now;
        public string? UsuarioAct { get; set; }
        public DateTime? FechaAct { get; set; }
    }
}
