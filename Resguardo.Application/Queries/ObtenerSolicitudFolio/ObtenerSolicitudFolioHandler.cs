using Microsoft.AspNetCore.Http;
using Resguardo.Application.Exceptions;
using Resguardo.Application.Interfaces;

namespace Resguardo.Application.Queries.ObtenerSolicitudFolio
{
    public class ObtenerSolicitudFolioHandler
    {
        private readonly ISolicitudQueryService _query;
        public ObtenerSolicitudFolioHandler(ISolicitudQueryService query)
        {
            _query = query;
        }
        public async Task<ObtenerSolicitudFolioResponse?> Ejecutar(string folio)
        {
            var folioFormat = !string.IsNullOrEmpty(folio) ? folio.Split('-')[0] : "";
            if (string.IsNullOrEmpty(folioFormat))
                throw new BusinessException(StatusCodes.Status400BadRequest.ToString(), "Debe ingresar el folio");

            var solicitud = await _query.Obtener(folioFormat);
            if (solicitud is null)
                throw new BusinessException(StatusCodes.Status400BadRequest.ToString(),
                    $"La solicitud {folioFormat} no existe");

            return solicitud;
        }
    }
}
