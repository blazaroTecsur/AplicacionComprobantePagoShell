namespace Resguardo.Application.Commands.CancelarServicio
{
    public class CancelarServicioCommand
    {
        public int IdServicioProv { get; set; }
        public string? Comentario { get; set; } = null!;
    }
}
