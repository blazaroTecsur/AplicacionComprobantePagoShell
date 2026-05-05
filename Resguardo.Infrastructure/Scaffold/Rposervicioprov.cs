using System;
using System.Collections.Generic;

namespace Resguardo.Infrastructure.Scaffold;

public partial class Rposervicioprov
{
    public int IdServicioProv { get; set; }

    public int IdServicio { get; set; }

    public int IdProveedor { get; set; }

    public int Cantidad { get; set; }

    public string Estado { get; set; } = null!;

    public virtual Rpogenerico IdProveedorNavigation { get; set; } = null!;

    public virtual Rposervicio IdServicioNavigation { get; set; } = null!;

    public virtual ICollection<Rpoefectivo> Rpoefectivos { get; set; } = new List<Rpoefectivo>();
}
