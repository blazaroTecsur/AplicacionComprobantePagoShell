namespace Resguardo.Application.Commands.CerrarServicio
{
    public class CerrarServicioCommand
    {
        public int IdServicioProv { get; set; }
        public List<CerrarServicioItem> Efectivos { get; set; } = new();
    }
}
