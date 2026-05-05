namespace Resguardo.Application.Commands.RegistrarSolicitud
{
    public class RegistrarSolicitudCommand
    {        
        public int IdFlujo { get; set; }
        public string NumSro { get; set; } = null!;        
        public string CodCapataz { get; set; } = null!;
        public string NomCapataz { get; set; } = null!;
        public string Celular { get; set; } = null!;
        public string TpoTrabajo { get; set; } = null!;        
        public List<RegistrarServicioItem> Servicios { get; set; } = new();
    }
}
