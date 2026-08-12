namespace Reciclaje.Dominio.Entidades;

public class Integracionsyteline
{
    public string RowPointer { get; set; } = null!;
    public string? Sitio { get; set; }
    public string? Sro { get; set; }
    public int? SroLine { get; set; }
    public int? SroOper { get; set; }
    public int? TransNum { get; set; }
    public DateTime? TransDate { get; set; }
    public string? Articulo { get; set; }
    public string? Estado { get; set; }
    public int? Posted { get; set; }
    public DateTime? FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}