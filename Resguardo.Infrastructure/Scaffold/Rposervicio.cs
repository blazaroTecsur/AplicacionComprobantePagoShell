using System;
using System.Collections.Generic;

namespace Resguardo.Infrastructure.Scaffold;

public partial class Rposervicio
{
    public int IdServicio { get; set; }

    public int IdSolicitud { get; set; }

    public int IdTpoServicio { get; set; }

    public DateOnly Fecha { get; set; }

    public string HraInicio { get; set; } = null!;

    public string HraFinal { get; set; } = null!;

    public bool DiaSig { get; set; }

    public string Coordenada { get; set; } = null!;

    public string Direccion { get; set; } = null!;

    public int Cantidad { get; set; }

    public string UsuarioReg { get; set; } = null!;

    public DateTime FechaReg { get; set; }

    public string? UsuarioAct { get; set; }

    public DateTime? FechaAct { get; set; }

    public int? CantidadBck { get; set; }

    public string? Comentario { get; set; }

    public string? HraAmplia { get; set; }

    public virtual Rposolicitud IdSolicitudNavigation { get; set; } = null!;

    public virtual Rpogenerico IdTpoServicioNavigation { get; set; } = null!;

    public virtual ICollection<Rposervicioprov> Rposervicioprovs { get; set; } = new List<Rposervicioprov>();
}
