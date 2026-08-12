namespace Reciclaje.Dominio.Entidades;

public partial class Logintegracionsyteline 
{
    public int LogId { get; set; }

    public DateTime? FechaEnvio { get; set; }

    public string? Trama { get; set; }

    public string? EstadoEnvio { get; set; }

    public string? Mensaje { get; set; }
}
