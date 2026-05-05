namespace Resguardo.Application.Queries.ConsultarServicio
{
    public class ConsultarServicioResponse
    {
        public int Id { get; set; }
        public string TpoServicio { get; set; } = null!;
        public DateOnly Fecha { get; set; }
        public string HraInicio { get; set; } = null!;
        public string HraFinal { get; set; } = null!;
        public bool DiaSig { get; set; }
        public string Coordenada { get; set; } = null!;
        public string Direccion { get; set; } = null!;
        public int Cantidad { get; set; }
        public string Proveedor { get; set; } = null!;
        public string Folio { get; set; } = null!;
        public string Estado { get; set; } = null!;
        public bool Asignar { get; set; }
        public bool Cerrar { get; set; }
        public bool Visual { get; set; }
        public bool Ampliar { get; set; }
        public bool Aprobar { get; set; }
    }
}