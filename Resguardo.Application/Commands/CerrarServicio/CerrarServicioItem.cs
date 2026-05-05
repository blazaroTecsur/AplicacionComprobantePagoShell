namespace Resguardo.Application.Commands.CerrarServicio
{
    public class CerrarServicioItem
    {
        public int IdEfectivo { get; set; }
        public string HraInicio { get; set; } = null!;
        public string HraFinal { get; set; } = null!;
        public bool Asistio { get; set; }
        public string? SroRefencia { get; set; }
        public string? Comentario { get; set; }
    }
}
