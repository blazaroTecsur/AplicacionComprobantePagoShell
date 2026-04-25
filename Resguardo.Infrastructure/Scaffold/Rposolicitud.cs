using System;
using System.Collections.Generic;

namespace Resguardo.Infrastructure.Scaffold;

public partial class Rposolicitud
{
    public int IdSolicitud { get; set; }

    public string Folio { get; set; } = null!;

    public int IdTipo { get; set; }

    public int IdEstado { get; set; }

    public int IdFlujo { get; set; }

    public string NumSro { get; set; } = null!;

    public string CodDpto { get; set; } = null!;

    public string NomDpto { get; set; } = null!;

    public string CodActv { get; set; } = null!;

    public string NomActv { get; set; } = null!;

    public string CodSupr { get; set; } = null!;

    public string NomSupr { get; set; } = null!;

    public string RucSctta { get; set; } = null!;

    public string NomSctta { get; set; } = null!;

    public string CodCapataz { get; set; } = null!;

    public string NomCapataz { get; set; } = null!;

    public DateTime FechaFoc { get; set; }

    public string Coordenada { get; set; } = null!;

    public string Direccion { get; set; } = null!;

    public string Celular { get; set; } = null!;

    public string TpoTrabajo { get; set; } = null!;

    public string? UsuarioApro { get; set; }

    public DateTime? FechaApro { get; set; }

    public string? Comentario { get; set; }

    public string? FolioRef { get; set; }

    public string UsuarioReg { get; set; } = null!;

    public DateTime FechaReg { get; set; }

    public string? UsuarioAct { get; set; }

    public DateTime? FechaAct { get; set; }

    public virtual Rpogenerico IdEstadoNavigation { get; set; } = null!;

    public virtual Rpogenerico IdFlujoNavigation { get; set; } = null!;

    public virtual Rpogenerico IdTipoNavigation { get; set; } = null!;

    public virtual ICollection<Rposervicio> Rposervicios { get; set; } = new List<Rposervicio>();
}
