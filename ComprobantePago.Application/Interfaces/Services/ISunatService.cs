using ComprobantePago.Application.DTOs.Comprobante.Response;

namespace ComprobantePago.Application.Interfaces.Services
{
    public interface ISunatService
    {
        Task<ValidacionSunatDto> ValidarComprobanteAsync(
            string numRuc,
            string codComp,
            string numeroSerie,
            string numero,
            string fechaEmision,
            decimal monto);
    }
}
