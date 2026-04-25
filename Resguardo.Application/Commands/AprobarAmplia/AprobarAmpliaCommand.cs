namespace Resguardo.Application.Commands.AprobarAmplia
{
    public class AprobarAmpliaCommand
    {
        public int IdServicioProv { get; set; }
        public int IdEfectivo { get; set; }
        public string EstAmplia { get; set; } = null!;
        public string? ComentApro { get; set; }
    }
}