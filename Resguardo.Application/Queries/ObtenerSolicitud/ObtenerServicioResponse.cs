namespace Resguardo.Application.Queries.ObtenerSolicitud
{
    public class ObtenerServicioResponse
    {
        public int Id { get; set; }
        public int IdTpoServicio { get; set; }
        public string TpoServicio { get; set; } = null!;
        public DateOnly Fecha { get; set; }
        public string HraInicio { get; set; } = null!;
        public string HraFinal { get; set; } = null!;
        public bool DiaSig { get; set; }
        public string Coordenada { get; set; } = null!;
        public string Direccion { get; set; } = null!;
        public int Cantidad { get; set; }
        public int? CantidadBck { get; set; }
    }
}
