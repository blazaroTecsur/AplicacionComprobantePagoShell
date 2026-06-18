namespace Reciclaje.Dominio.Entidades;

public partial class Tareaordencompra
{
    public int Id { get; set; }

    public short Anno { get; set; }

    public byte Mes { get; set; }

    public string NombrePo { get; set; } = null!;

    public string? Sitio { get; set; }

    public DateTime FechaCreacion { get; set; }

    public string UsuarioCreacion { get; set; } = null!;

    public DateTime? FechaModificacion { get; set; }

    public string? UsuarioModificacion { get; set; }

    public string? Estado { get; set; }

    public string? UidSyteLine { get; set; }

    public int? UltimaLinea { get; set; }

}