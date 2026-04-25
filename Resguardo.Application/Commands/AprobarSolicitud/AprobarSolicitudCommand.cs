namespace Resguardo.Application.Commands.AprobarSolicitud
{
    public record AprobarSolicitudCommand
    {
        public int Id { get; set; }
        public string CodEstado { get; set; } = null!;
        public string? Comentario { get; set; }
        public List<AprobarServicioItem> Servicios { get; set; } = new();
    }
}