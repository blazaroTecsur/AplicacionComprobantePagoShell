namespace Resguardo.Application.Commands.EditarSolicitud
{
    public class EditarSolicitudCommand
    {
        public int Id { get; set; }
        public string CodCapataz { get; set; } = null!;
        public string NomCapataz { get; set; } = null!;
        public string Celular { get; set; } = null!;        
        public List<EditarServicioItem> Servicios { get; set; } = new();
    }
}