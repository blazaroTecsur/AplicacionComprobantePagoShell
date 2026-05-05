namespace Resguardo.Application.Queries.ListarServicio
{
    public class ListarServicioResponse
    {
        public int Id { get; set; }
        public int IdSolicitud { get; set; }        
        public string TpoServicio { get; set; } = null!;
        public DateOnly Fecha { get; set; }
        public string HraInicio { get; set; } = null!;
        public string HraFinal { get; set; } = null!;
        public bool DiaSig { get; set; }
        public string Coordenada { get; set; } = null!;
        public string Direccion { get; set; } = null!;
        public int Cantidad { get; set; }
        public List<ListarServicioProvItem> ServicioProvs { get; set; } = new();
    }
}