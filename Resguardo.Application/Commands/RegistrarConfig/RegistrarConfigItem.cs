namespace Resguardo.Application.Commands.RegistrarConfig
{
    public class RegistrarConfigItem
    {
        public int Id { get; set; }        
        public string CodDpto { get; set; } = null!;
        public int Diurno { get; set; }
        public int Nocturno { get; set; }
        public int IdTpoServicio { get; set; }        
    }
}