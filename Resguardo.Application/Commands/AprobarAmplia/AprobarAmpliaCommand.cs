namespace Resguardo.Application.Commands.AprobarAmplia
{
    public class AprobarAmpliaCommand
    {        
        public int IdEfectivo { get; set; }
        public string EstAmplia { get; set; } = null!;
        public string? ComentApro { get; set; }
    }
}