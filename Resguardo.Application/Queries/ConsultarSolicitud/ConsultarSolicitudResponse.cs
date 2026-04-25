namespace Resguardo.Application.Queries.ConsultarSolicitud
{
    public class ConsultarSolicitudResponse
    {
        public bool Modifica { get; set; }
        public bool Aprueba { get; set; }
        public bool Confirma { get; set; }
        public bool Edita { get; set; }        
        public int Id { get; set; }
        public string Folio { get; set; } = null!;
        public string Tipo { get; set; } = null!;
        public string Estado { get; set; } = null!;        
        public string Flujo { get; set; } = null!;
        public string NumSro { get; set; } = null!;
        public string Sctta { get; set; } = null!;
        public string? HrasSolicitadas { get; set; }
        public string? FolioRef { get; set; }
        public string UsuarioReg { get; set; } = null!;
        public DateTime FechaReg { get; set; }
    }
}
