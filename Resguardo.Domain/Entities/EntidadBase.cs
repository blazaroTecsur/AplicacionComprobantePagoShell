namespace Resguardo.Domain.Entities
{
    public abstract class EntidadBase
    {
        public int Id { get; set; }
        public string UsuarioReg { get; set; } = null!;
        public DateTime FechaReg { get; set; }
        public string? UsuarioAct { get; set; }
        public DateTime? FechaAct { get; set; }
    }
}
