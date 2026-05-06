namespace Resguardo.Application.DTOs.Infor
{
    public class ObtenerOrdenResponse
    {
        public string Id { get; set; } = null!;
        public string NumSro { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public string CodDpto { get; set; } = null!;
        public string NomDpto { get; set; } = null!;
        public string RucSctta { get; set; } = null!;
        public string NomSctta { get; set; } = null!;
        public string CodActv { get; set; } = null!;
        public string NomActv { get; set; } = null!;
        public string CodSupr { get; set; } = null!;
        public string NomSupr { get; set; } = null!;
        public DateTime FechaFoc { get; set; }
        public string Estado { get; set; } = null!;
        public string Coordenada { get; set; } = null!;
        public string Direccion { get; set; } = null!;
    }
}