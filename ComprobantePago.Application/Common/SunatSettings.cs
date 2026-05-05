namespace ComprobantePago.Application.Common
{
    public class SunatSettings
    {
        public string ApiUrl { get; set; } = null!;
        public string TokenUrl { get; set; } = null!;
        public string ClientId { get; set; } = null!;
        public string ClientSecret { get; set; } = null!;
        public string Scope { get; set; } = null!;
        public string RucEmpresa { get; set; } = null!;
    }
}
