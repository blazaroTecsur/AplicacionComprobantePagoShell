using System;
using System.Collections.Generic;

namespace Resguardo.Infrastructure.Scaffold;

public partial class Rpoconfig
{
    public int IdConfig { get; set; }

    public DateOnly Fecha { get; set; }

    public string CodActividad { get; set; } = null!;

    public string DscActividad { get; set; } = null!;

    public int IdTpoServicio { get; set; }

    public int Cantidad { get; set; }

    public string UsuarioReg { get; set; } = null!;

    public DateTime FechaReg { get; set; }

    public string? UsuarioAct { get; set; }

    public DateTime? FechaAct { get; set; }

    public virtual Rpogenerico IdTpoServicioNavigation { get; set; } = null!;
}
