namespace Resguardo.Application.Commands.AsignarEfectivo
{
    public class AsignarEfectivoCommand
    {
        public int IdServicioProv { get; set; }
        public List<AsignarEfectivoItem> Efectivos { get; set; } = new();
    }
}