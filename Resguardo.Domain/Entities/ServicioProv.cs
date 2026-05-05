namespace Resguardo.Domain.Entities
{
    public class ServicioProv : EntidadBase
    {
        public int IdServicio { get; set; }
        public int IdProveedor { get; set; }
        public int Cantidad { get; set; }        
        public string Estado { get; set; } = null!;
        public virtual Generico ProveedorNav { get; set; } = null!;
        public virtual Servicio ServicioNav { get; set; } = null!;
        public virtual ICollection<Efectivo> Efectivos { get; set; } = new List<Efectivo>();
    }
}
