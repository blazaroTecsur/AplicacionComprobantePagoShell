namespace Resguardo.Application.Commands.RegistrarSolicitud
{
    public class RegistrarServicioItem
    {
        public int IdTpoServicio { get; set; }
        public DateOnly Fecha { get; set; }
        public string HraInicio { get; set; } = null!;
        public string HraFinal { get; set; } = null!;
        public bool DiaSig { get; set; }
        public bool ZonaTraf { get; set; }
        public string Coordenada { get; set; } = null!;
        public string Direccion { get; set; } = null!;
        public int Cantidad { get; set; }
    }
}
