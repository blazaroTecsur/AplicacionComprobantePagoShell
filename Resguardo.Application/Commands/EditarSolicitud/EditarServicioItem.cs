namespace Resguardo.Application.Commands.EditarSolicitud
{
    public class EditarServicioItem
    {
        public int Id { get; set; }        
        public string HraInicio { get; set; } = null!;
        public string HraFinal { get; set; } = null!;
        public bool DiaSig { get; set; }        
    }
}
