namespace Resguardo.Domain.Entities;

public partial class Aprobador: EntidadBase
{
    public string CodSocio { get; set; } = null!;
    public string NomSocio { get; set; } = null!;
    public string CodUnidad { get; set; } = null!;
    public string DscUnidad { get; set; } = null!;
}
