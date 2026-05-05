using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Resguardo.Application.Common;
using Resguardo.Application.Common.Interfaces;
using Resguardo.Application.Exceptions;
using Resguardo.Application.Interfaces;

namespace Resguardo.Application.Queries.ReporteEfectivo
{
    public class ReporteEfectivoHandler
    {
        private readonly IReporteQueryService _query;
        private readonly IValidacionService _validacion;
        public ReporteEfectivoHandler(IReporteQueryService query, IValidacionService validacion)
        {
            _query = query;
            _validacion = validacion;
        }
        public async Task<byte[]> Ejecutar(ReporteEfectivoQuery filtro)
        {
            ValidationResult resultado = _validacion.ValidarRangoReporte(filtro.FechaDsd, filtro.FechaHst);
            if (!resultado.EsValido)
            {
                throw new BusinessException(StatusCodes.Status400BadRequest.ToString(),
                    resultado.Mensaje);
            }

            var efectivos = await _query.ReporteEfectivos(filtro);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Reporte");

            worksheet.Cell(1, 1).Value = "Fecha de Servicio";
            worksheet.Cell(1, 2).Value = "Nombres";
            worksheet.Cell(1, 3).Value = "Teléfono";
            worksheet.Cell(1, 4).Value = "Dirección";
            worksheet.Cell(1, 5).Value = "Coordenada";
            worksheet.Cell(1, 6).Value = "Tipo de Servicio";
            worksheet.Cell(1, 7).Value = "Hra Inicio";
            worksheet.Cell(1, 8).Value = "Hra Final";
            worksheet.Cell(1, 9).Value = "Hra Inicio Real";
            worksheet.Cell(1, 10).Value = "Hra Final Real";
            worksheet.Cell(1, 11).Value = "Hra Ampliada";
            worksheet.Cell(1, 12).Value = "Solicitante";
            worksheet.Cell(1, 13).Value = "Cód. Unidad";
            worksheet.Cell(1, 14).Value = "Departamento";
            worksheet.Cell(1, 15).Value = "Actividad";
            worksheet.Cell(1, 16).Value = "Subcontratista";
            worksheet.Cell(1, 17).Value = "Capataz";
            worksheet.Cell(1, 18).Value = "Celular";
            worksheet.Cell(1, 19).Value = "Núm. SRO";
            worksheet.Cell(1, 20).Value = "Folio";

            int row = 2;

            foreach (var efectivo in efectivos)
            {
                worksheet.Cell(row, 1).Value = efectivo.Fecha.ToString("dd/MM/yyyy");
                worksheet.Cell(row, 2).Value = efectivo.Nombres;
                worksheet.Cell(row, 3).Value = efectivo.Telefono;
                worksheet.Cell(row, 4).Value = efectivo.Direccion;
                worksheet.Cell(row, 5).Value = efectivo.Coordenada;
                worksheet.Cell(row, 6).Value = efectivo.TipoServicio;
                worksheet.Cell(row, 7).Value = efectivo.HraInicio;
                worksheet.Cell(row, 8).Value = efectivo.HraFinal;
                worksheet.Cell(row, 9).Value = efectivo.HraInicioReal;
                worksheet.Cell(row, 10).Value = efectivo.HraFinalReal;
                worksheet.Cell(row, 11).Value = efectivo.HraAmplia;
                worksheet.Cell(row, 12).Value = efectivo.UsuarioReg;
                worksheet.Cell(row, 13).Value = efectivo.CodDpto;
                worksheet.Cell(row, 14).Value = efectivo.NomDpto;
                worksheet.Cell(row, 15).Value = efectivo.NomActv;
                worksheet.Cell(row, 16).Value = efectivo.NomSctta;
                worksheet.Cell(row, 17).Value = efectivo.NomCapataz;
                worksheet.Cell(row, 18).Value = efectivo.Celular;
                worksheet.Cell(row, 19).Value = efectivo.NumSro;
                worksheet.Cell(row, 20).Value = efectivo.Folio;
                row++;
            }

            // Estilo opcional
            var range = worksheet.Range(1, 1, 1, 20);
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
