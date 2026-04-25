namespace Resguardo.Application.Queries.ListarEfectivos
{
    public class ListarEfectivoResponse
    {
        public int IdEfectivo { get; set; }
        public string Dni { get; set; } = null!;
        public string Nombres { get; set; } = null!;
        public string Apellidos { get; set; } = null!;
        public string? Telefono { get; set; }
        public string? HraInicio { get; set; }
        public string? HraFinal { get; set; }
        public bool? Asistio { get; set; }
        public string? SroRefencia { get; set; }
        public string? Comentario { get; set; }
        public string? HraAmplia { get; set; }
        public string? EstAmplia { get; set; }
        public string? UsuarioApro { get; set; }
        public DateTime? FechaApro { get; set; }
        public string? ComentApro { get; set; }
    }
}