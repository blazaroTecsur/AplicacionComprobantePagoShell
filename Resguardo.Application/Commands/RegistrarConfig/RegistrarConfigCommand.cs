namespace Resguardo.Application.Commands.RegistrarConfig
{
    public class RegistrarConfigCommand
    {
        public DateOnly Fecha { get; set; }
        public List<RegistrarConfigItem> Configs { get; set; } = new ();
    }
}
