using System;
using System.Collections.Generic;

namespace Resguardo.Infrastructure.Scaffold;

public partial class Rpoaprobador
{
    public int IdAprobador { get; set; }

    public string CodSocio { get; set; } = null!;

    public string NomSocio { get; set; } = null!;

    public string CodUnidad { get; set; } = null!;

    public string DscUnidad { get; set; } = null!;

    public string UsuarioReg { get; set; } = null!;

    public DateTime FechaReg { get; set; }

    public string? UsuarioAct { get; set; }

    public DateTime? FechaAct { get; set; }
}
