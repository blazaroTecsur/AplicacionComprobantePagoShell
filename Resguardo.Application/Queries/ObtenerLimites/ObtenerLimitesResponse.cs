namespace Resguardo.Application.Queries.ObtenerConfig
{
    public class ObtenerLimitesResponse
    {
        public string TpoServicio { get; set; } = null!;
        public int DiurnoConfig { get; set; }
        public int NocturnoConfig { get; set; }
        public int DiurnoSolicitado { get; set; }
        public int NocturnoSolicitado { get; set; }
    }
}
