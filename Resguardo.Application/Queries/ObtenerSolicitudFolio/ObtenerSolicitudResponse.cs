namespace Resguardo.Application.Queries.ObtenerSolicitudFolio
{
    public class ObtenerSolicitudFolioResponse
    {        
        public string Folio { get; set; } = null!;
        public string Estado { get; set; }
        public string Flujo { get; set; }
        public string NumSro { get; set; } = null!;
        public string Dpto { get; set; } = null!;        
        public string Actv { get; set; } = null!;        
        public string Sctta { get; set; } = null!;        
        public string Capataz { get; set; } = null!;        
        public string Celular { get; set; } = null!;
        public string TpoTrabajo { get; set; } = null!;        
    }
}
