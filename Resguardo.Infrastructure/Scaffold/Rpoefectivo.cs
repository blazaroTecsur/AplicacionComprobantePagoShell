using System;
using System.Collections.Generic;

namespace Resguardo.Infrastructure.Scaffold;

public partial class Rpoefectivo
{
    public int IdEfectivo { get; set; }

    public int IdServicioProv { get; set; }

    public int IdPersonal { get; set; }

    public string? Telefono { get; set; }

    public string? HraInicio { get; set; }

    public string? HraFinal { get; set; }

    public string UsuarioReg { get; set; } = null!;

    public DateTime FechaReg { get; set; }

    public string? UsuarioAct { get; set; }

    public DateTime? FechaAct { get; set; }

    public bool? Asistio { get; set; }

    public string? Comentario { get; set; }

    public virtual Rpopersonal IdPersonalNavigation { get; set; } = null!;

    public virtual Rposervicioprov IdServicioProvNavigation { get; set; } = null!;
}
