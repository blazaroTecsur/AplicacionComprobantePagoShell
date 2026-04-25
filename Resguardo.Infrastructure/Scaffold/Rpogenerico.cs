using System;
using System.Collections.Generic;

namespace Resguardo.Infrastructure.Scaffold;

public partial class Rpogenerico
{
    public int IdGenerico { get; set; }

    public string Tipo { get; set; } = null!;

    public string Codigo { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public string UsuarioReg { get; set; } = null!;

    public DateTime FechaReg { get; set; }

    public string? UsuarioAct { get; set; }

    public DateTime? FechaAct { get; set; }

    public virtual ICollection<Rpoconfig> Rpoconfigs { get; set; } = new List<Rpoconfig>();

    public virtual ICollection<Rposervicioprov> Rposervicioprovs { get; set; } = new List<Rposervicioprov>();

    public virtual ICollection<Rposervicio> Rposervicios { get; set; } = new List<Rposervicio>();

    public virtual ICollection<Rposolicitud> RposolicitudIdEstadoNavigations { get; set; } = new List<Rposolicitud>();

    public virtual ICollection<Rposolicitud> RposolicitudIdFlujoNavigations { get; set; } = new List<Rposolicitud>();

    public virtual ICollection<Rposolicitud> RposolicitudIdTipoNavigations { get; set; } = new List<Rposolicitud>();
}
