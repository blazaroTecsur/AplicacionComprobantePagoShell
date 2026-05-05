namespace ComprobantePago.Domain.Entities
{
    public class Moneda
    {
        public int IdMoneda { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string? Simbolo { get; set; }
        public bool Activo { get; set; } = true;
        public string UsuarioReg { get; set; } = string.Empty;
        public DateTime FechaReg { get; set; } = DateTime.Now;
        public string? UsuarioAct { get; set; }
        public DateTime? FechaAct { get; set; }
    }
}
