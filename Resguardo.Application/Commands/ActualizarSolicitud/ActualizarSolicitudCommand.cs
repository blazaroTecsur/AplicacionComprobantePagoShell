namespace Resguardo.Application.Commands.ActualizarSolicitud
{
    public class ActualizarSolicitudCommand
    {
        public int Id { get; set; }
        public string CodCapataz { get; set; } = null!;
        public string NomCapataz { get; set; } = null!;
        public string Celular { get; set; } = null!;
        public string TpoTrabajo { get; set; } = null!;
        public List<ActualizarServicioItem> Servicios { get; set; } = new();
    }
}