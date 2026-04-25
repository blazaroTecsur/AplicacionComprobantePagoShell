namespace Resguardo.Application.Commands.AmpliarServicio
{
    public class AmpliarServicioCommand
    {
        public int IdServicioProv { get; set; }
        public List<AmpliarServicioItem> Efectivos { get; set; } = new();
    }
}