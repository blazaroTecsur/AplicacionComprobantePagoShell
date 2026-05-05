using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Resguardo.Application.Common;
using Resguardo.Application.Common.Interfaces;
using Resguardo.Application.Exceptions;
using Resguardo.Application.Interfaces;

namespace Resguardo.Application.Queries.ReporteSolicitud
{
    public class ReporteSolicitudHandler
    {
        private readonly IReporteQueryService _query;
        private readonly IValidacionService _validacion;
        public ReporteSolicitudHandler(IReporteQueryService query, IValidacionService validacion)
        {
            _query = query;
            _validacion = validacion;
        }
        public async Task<byte[]> Ejecutar(ReporteSolicitudQuery filtro)
        {
            ValidationResult resultado = _validacion.ValidarRangoReporte(filtro.FechaDsd, filtro.FechaHst);
            if (!resultado.EsValido)
            {
                throw new BusinessException(StatusCodes.Status400BadRequest.ToString(),
                    resultado.Mensaje);
            }

            var solicitudes = await _query.ReporteSolicitudes(filtro);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Reporte");
            
            worksheet.Cell(1, 1).Value = "Fecha Reg";
            worksheet.Cell(1, 2).Value = "Flujo";
            worksheet.Cell(1, 3).Value = "Folio";
            worksheet.Cell(1, 4).Value = "Núm. SRO";
            worksheet.Cell(1, 5).Value = "Estado";
            worksheet.Cell(1, 6).Value = "Ruc";
            worksheet.Cell(1, 7).Value = "Subcontratista";
            worksheet.Cell(1, 8).Value = "Supervisor";
            worksheet.Cell(1, 9).Value = "Capataz";
            worksheet.Cell(1, 10).Value = "Celular";
            worksheet.Cell(1, 11).Value = "Tipo de Trabajo";
            worksheet.Cell(1, 12).Value = "Cód. Unidad";
            worksheet.Cell(1, 13).Value = "Departamento";
            worksheet.Cell(1, 14).Value = "Aprobador";
            worksheet.Cell(1, 15).Value = "F. Aprobación";
            worksheet.Cell(1, 16).Value = "Tipo de Servicio";
            worksheet.Cell(1, 17).Value = "Fecha";
            worksheet.Cell(1, 18).Value = "Hra Inicio";
            worksheet.Cell(1, 19).Value = "Hra Final";
            worksheet.Cell(1, 20).Value = "Turno";
            worksheet.Cell(1, 21).Value = "Cantidad";
            worksheet.Cell(1, 22).Value = "Dirección";
            worksheet.Cell(1, 23).Value = "Coordenada";
            worksheet.Cell(1, 24).Value = "Comentario (Aprobación)";

            int row = 2;

            foreach (var solicitud in solicitudes)
            {                
                worksheet.Cell(row, 1).Value = solicitud.FechaReg;
                worksheet.Cell(row, 2).Value = solicitud.Flujo; 
                worksheet.Cell(row, 3).Value = solicitud.Folio; 
                worksheet.Cell(row, 4).Value = solicitud.NumSro;
                worksheet.Cell(row, 5).Value = solicitud.Estado;
                worksheet.Cell(row, 6).Value = solicitud.RucSctta;
                worksheet.Cell(row, 7).Value = solicitud.NomSctta;
                worksheet.Cell(row, 8).Value = solicitud.NomSupr; 
                worksheet.Cell(row, 9).Value = solicitud.NomCapataz; 
                worksheet.Cell(row, 10).Value = solicitud.Celular; 
                worksheet.Cell(row, 11).Value = solicitud.TpoTrabajo; 
                worksheet.Cell(row, 12).Value = solicitud.CodDpto; 
                worksheet.Cell(row, 13).Value = solicitud.NomDpto; 
                worksheet.Cell(row, 14).Value = solicitud.UsuarioApro; 
                worksheet.Cell(row, 15).Value = solicitud.FechaApro; 
                worksheet.Cell(row, 16).Value = solicitud.TipoServicio; 
                worksheet.Cell(row, 17).Value = solicitud.Fecha.ToString("dd/MM/yyyy"); 
                worksheet.Cell(row, 18).Value = solicitud.HraInicio;
                worksheet.Cell(row, 19).Value = solicitud.HraFinal;
                worksheet.Cell(row, 20).Value = solicitud.Turno; 
                worksheet.Cell(row, 21).Value = solicitud.Cantidad;
                worksheet.Cell(row, 22).Value = solicitud.Direccion;
                worksheet.Cell(row, 23).Value = solicitud.Coordenada;
                worksheet.Cell(row, 24).Value = solicitud.Comentario;
                row++;
            }

            // Estilo opcional
            var range = worksheet.Range(1, 1, 1, 24);
            range.Style.Font.Bold = true;
            // Ajustar columnas
            worksheet.Columns().AdjustToContents();
            // Guardar en memoria
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return stream.ToArray();            
        }
    }
}