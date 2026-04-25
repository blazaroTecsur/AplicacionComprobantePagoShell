namespace Resguardo.Application.Queries.ListarServicio
{
    public class ListarServicioProvItem
    {
        public int Id { get; set; }
        public int IdServicio { get; set; }
        public int IdProveedor { get; set; }
        public string Proveedor { get; set; } = null!;                
        public int Cantidad { get; set; }
    }
}
