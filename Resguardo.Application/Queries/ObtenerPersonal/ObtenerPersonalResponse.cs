namespace Resguardo.Application.Queries.ObtenerPersonal
{
    public class ObtenerPersonalResponse
    {
        public string Nombres { get; set; } = null!;
        public string Apellidos { get; set; } = null!;
        public string? Telefono { get; set; }
        public int IdPersonal { get; set; }
    }
}
