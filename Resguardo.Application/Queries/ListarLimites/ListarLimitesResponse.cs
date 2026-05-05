namespace Resguardo.Application.Queries.ListarLimites
{
    public class ListarLimitesResponse
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string CodDpto { get; set; } = null!;
        public string NomDpto { get; set; } = null!;
        public int Diurno { get; set; } 
        public int Nocturno { get; set; }
        public int IdTpoServicio { get; set; }
        public string TpoServicio { get; set; } = null!;        
    }
}
