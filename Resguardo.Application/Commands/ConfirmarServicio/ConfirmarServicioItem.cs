namespace Resguardo.Application.Commands.ConfirmarServicio
{
    public class ConfirmarServicioItem
    {
        public int Id { get; set; }
        public int IdServicio { get; set; }
        public int IdProveedor { get; set; }
        public int Cantidad { get; set; }
    }
}