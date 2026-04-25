namespace Resguardo.Application.Commands.ConfirmarServicio
{
    public class ConfirmarServicioCommand
    {
        public int IdSolicitud { get; set; }
        public List<ConfirmarServicioItem> Servicios { get; set; } = new();
    }
}
