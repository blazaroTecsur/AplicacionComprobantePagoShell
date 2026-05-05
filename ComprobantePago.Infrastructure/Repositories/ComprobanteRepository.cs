using ComprobantePago.Application.Commands.Comprobante;
using ComprobantePago.Application.Commands.Imputacion;
using ComprobantePago.Application.DTOs.Comprobante.Response;
using ComprobantePago.Application.Exceptions;
using ComprobantePago.Application.Interfaces;
using ComprobantePago.Application.Interfaces.Repositories;
using ComprobantePago.Application.Interfaces.Services;
using ComprobantePago.Domain.Entities;
using ComprobantePago.Infrastructure.Persistence;
using ComprobantePago.Infrastructure.Services;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.IO.Compression;

namespace ComprobantePago.Infrastructure.Repositories
{
    public class ComprobanteRepository(
        AppDbContext contexto,
        IUnitOfWork unitOfWork,
        ISunatService sunatService,
        XmlComprobanteService xmlService,
        PdfComprobanteService pdfService,
        IUsuarioContexto usuario,
        ILogger<ComprobanteRepository> logger)
        : RepositorioBase<Comprobante>(contexto), IComprobanteRepository
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ISunatService _sunatService = sunatService;
        private readonly XmlComprobanteService _xmlService = xmlService;
        private readonly PdfComprobanteService _pdfService = pdfService;
        private readonly IUsuarioContexto _usuario = usuario;
        private readonly ILogger<ComprobanteRepository> _logger = logger;

        // ── Generar Folio ─────────────────────────
        public async Task<string> GenerarFolioAsync()
        {
            var anio = DateTime.Now.Year.ToString();
            var mes = DateTime.Now.Month.ToString("D2");
            var correlativo = await ObtenerCorrelativoAsync(anio, mes);
            return $"{anio}{mes}{correlativo}";
        }

        private async Task<string> ObtenerCorrelativoAsync(string anio, string mes)
        {
            var prefijo = $"{anio}{mes}";

            var maxFolio = await _contexto.Comprobantes
                .Where(x => x.Folio.StartsWith(prefijo))
                .OrderByDescending(x => x.Folio)
                .Select(x => x.Folio)
                .FirstOrDefaultAsync();

            if (maxFolio is null)
                return "0001";

            return (int.TryParse(maxFolio[prefijo.Length..], out var num) ? num + 1 : 1)
                .ToString("D4");
        }

        // ── Guardar Comprobante ───────────────────
        public async Task<string> GuardarAsync(RegistrarComprobanteCommand command)
        {
            var dto = command.Comprobante;
            var folio = string.IsNullOrWhiteSpace(dto.Folio)
                ? await GenerarFolioAsync()
                : dto.Folio;
            _logger.LogInformation("Guardando comprobante folio {Folio}", folio);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var existente = await _contexto.Comprobantes
                    .FirstOrDefaultAsync(x => x.Folio == folio);

                if (existente != null)
                {
                    existente.RucReceptor          = dto.Ruc;
                    existente.RazonSocialReceptor  = dto.RazonSocial;
                    existente.TipoDocumento        = dto.TipoDocumento;
                    existente.TipoSunat            = dto.TipoSunat;
                    existente.Serie                = dto.Serie;
                    existente.Numero               = dto.Numero;
                    existente.FechaEmision         = ParseFecha(dto.FechaEmision);
                    existente.FechaRecepcion       = ParseFechaNullable(dto.FechaRecepcion);
                    existente.FechaVencimiento     = ParseFechaNullable(dto.FechaVencimiento);
                    existente.Moneda               = dto.Moneda;
                    existente.TasaCambio           = dto.TasaCambio;
                    existente.LugarPago            = dto.LugarPago;
                    existente.PlazoPago            = dto.PlazoPago;
                    existente.RucBeneficiario      = dto.RucBenef;
                    existente.RazonSocialBenef     = dto.RazonSocial;
                    existente.Observacion          = dto.Observacion;
                    existente.OrdenCompra          = dto.OrdenCompra;
                    existente.FactMultiple         = dto.FactMultiple;
                    existente.TieneDetraccion      = dto.TieneDetraccion;
                    existente.TipoDetraccion       = dto.TipoDetraccion;
                    existente.PorcentajeDetraccion = dto.PorcentajeDetraccion;
                    existente.MontoDetraccion      = dto.MontoDetraccion;
                    existente.ConstanciaDeposito   = dto.ConstanciaDeposito;
                    existente.FechaDeposito        = ParseFechaNullable(dto.FechaDeposito);
                    existente.EsDocumentoElectronico = dto.EsDocumentoElectronico == "S";
                    existente.MontoNeto            = dto.MontoNeto;
                    existente.MontoExento          = dto.MontoExento;
                    existente.PorcentajeIGV        = dto.PorcentajeIGV;
                    existente.MontoIGVCredito      = dto.MontoIGVCredito;
                    existente.MontoTotal           = dto.MontoTotal;
                    existente.MontoBruto           = dto.MontoBruto;
                    existente.MontoRetencion       = dto.MontoRetencion;
                    existente.EsEmpleado           = dto.EsEmpleado;
                    existente.EmpleadoCodigo       = dto.EmpleadoCodigo;
                    existente.EmpleadoNombre       = dto.EmpleadoNombre;
                    existente.CodigoEstado         = "REGISTRADO";
                    existente.UsuarioAct           = _usuario.Correo;
                    existente.FechaAct             = DateTime.Now;
                }
                else
                {
                    var comprobante = new Comprobante
                    {
                        Folio                  = folio,
                        RucReceptor            = dto.Ruc,
                        RazonSocialReceptor    = dto.RazonSocial,
                        TipoDocumento          = dto.TipoDocumento,
                        TipoSunat              = dto.TipoSunat,
                        Serie                  = dto.Serie,
                        Numero                 = dto.Numero,
                        FechaEmision           = ParseFecha(dto.FechaEmision),
                        FechaRecepcion         = ParseFechaNullable(dto.FechaRecepcion),
                        FechaVencimiento       = ParseFechaNullable(dto.FechaVencimiento),
                        Moneda                 = dto.Moneda,
                        TasaCambio             = dto.TasaCambio,
                        LugarPago              = dto.LugarPago,
                        PlazoPago              = dto.PlazoPago,
                        RucBeneficiario        = dto.RucBenef,
                        RazonSocialBenef       = dto.RazonSocial,
                        Observacion            = dto.Observacion,
                        OrdenCompra            = dto.OrdenCompra,
                        FactMultiple           = dto.FactMultiple,
                        TieneDetraccion        = dto.TieneDetraccion,
                        TipoDetraccion         = dto.TipoDetraccion,
                        PorcentajeDetraccion   = dto.PorcentajeDetraccion,
                        MontoDetraccion        = dto.MontoDetraccion,
                        ConstanciaDeposito     = dto.ConstanciaDeposito,
                        FechaDeposito          = ParseFechaNullable(dto.FechaDeposito),
                        EsDocumentoElectronico = dto.EsDocumentoElectronico == "S",
                        MontoNeto              = dto.MontoNeto,
                        MontoExento            = dto.MontoExento,
                        PorcentajeIGV          = dto.PorcentajeIGV,
                        MontoIGVCredito        = dto.MontoIGVCredito,
                        MontoTotal             = dto.MontoTotal,
                        MontoBruto             = dto.MontoBruto,
                        MontoRetencion         = dto.MontoRetencion,
                        EsEmpleado             = dto.EsEmpleado,
                        EmpleadoCodigo         = dto.EmpleadoCodigo,
                        EmpleadoNombre         = dto.EmpleadoNombre,
                        CodigoEstado           = "REGISTRADO",
                        RolDigitacion          = _usuario.Correo,
                        FechaDigitacion        = DateTime.Now,
                        UsuarioReg             = _usuario.Correo,
                        FechaReg               = DateTime.Now
                    };
                    await _entidades.AddAsync(comprobante);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }

            return folio;
        }

        // ── Enviar ────────────────────────────────
        public async Task EnviarAsync(EnviarComprobanteCommand command)
        {
            var c = await ObtenerPorFolioAsync(command.Comprobante.Folio);
            c.CodigoEstado = "ENVIADO";
            c.UsuarioAct   = _usuario.Correo;
            c.FechaAct     = DateTime.Now;
            await _unitOfWork.SaveChangesAsync();
        }

        // ── Firmar (Autorizador) ──────────────────
        public async Task FirmarAsync(FirmarComprobanteCommand command)
        {
            var c = await ObtenerPorFolioAsync(command.Comprobante.Folio);
            c.CodigoEstado    = "AUTORIZADO";
            c.RolAutorizacion = _usuario.Correo;
            c.FechaAutorizacion = DateTime.Now;
            c.UsuarioAct      = _usuario.Correo;
            c.FechaAct        = DateTime.Now;
            await _unitOfWork.SaveChangesAsync();
        }

        // ── Aprobar (Aprobador) ───────────────────
        public async Task AprobarAsync(AprobarComprobanteCommand command)
        {
            var c = await ObtenerPorFolioAsync(command.Comprobante.Folio);
            c.CodigoEstado    = "APROBADO";
            c.RolAprobacion   = _usuario.Correo;
            c.FechaAprobacion = DateTime.Now;
            c.UsuarioAct      = _usuario.Correo;
            c.FechaAct        = DateTime.Now;
            await _unitOfWork.SaveChangesAsync();
        }

        // ── Anular ────────────────────────────────
        public async Task AnularAsync(AnularComprobanteCommand command)
        {
            var c = await ObtenerPorFolioAsync(command.Comprobante.Folio);
            c.CodigoEstado   = "ANULADO";
            c.RolAnulacion   = _usuario.Correo;
            c.FechaAnulacion = DateTime.Now;
            c.UsuarioAct     = _usuario.Correo;
            c.FechaAct       = DateTime.Now;
            await _unitOfWork.SaveChangesAsync();
        }

        // ── Derivar ───────────────────────────────
        public async Task DerivarAsync(DerivarComprobanteCommand command)
        {
            var c = await ObtenerPorFolioAsync(command.Comprobante.Folio);
            c.EstaDerivado    = true;
            c.CodigoEstado    = "DERIVADO SYT";
            c.UsuarioAct      = "SYSTEM";
            c.FechaAct        = DateTime.Now;
            if (command.VoucherSyteline.HasValue)
                c.VoucherSyteline = command.VoucherSyteline.Value;
            await _unitOfWork.SaveChangesAsync();
        }

        // ── Validar XML SUNAT ─────────────────────
        public async Task<ValidacionSunatDto> ValidarXmlSunatAsync(IFormFile archivo)
        {
            using var ms = new MemoryStream();
            await archivo.CopyToAsync(ms);
            var contenidoXml = ms.ToArray();

            using var stream = new MemoryStream(contenidoXml);
            var datos = _xmlService.LeerDatosXml(stream);

            var resultado = await _sunatService.ValidarComprobanteAsync(
                numRuc:       datos.RucProveedor,
                codComp:      datos.TipoSunat,
                numeroSerie:  datos.Serie,
                numero:       datos.Numero,
                fechaEmision: datos.FechaEmision,
                monto:        datos.MontoTotal);

            if (resultado.CodigoEstado == "1" || resultado.CodigoEstado == "3")
            {
                resultado.Datos = datos;
                resultado.Folio = await GenerarFolioAsync();
                await GuardarDocumentosAsync(resultado.Folio,
                    [(contenidoXml, archivo.FileName, "XML", "XML_SUNAT")]);
            }

            return resultado;
        }

        // ── Validar PDF SUNAT ─────────────────────
        public async Task<ValidacionSunatDto> ValidarPdfSunatAsync(IFormFile archivo)
        {
            var texto = await _pdfService.ExtraerTextoPdfAsync(archivo);

            if (string.IsNullOrWhiteSpace(texto))
                return new ValidacionSunatDto
                {
                    Exito       = false,
                    EstadoSunat = "ERROR",
                    Motivo      = "No se pudo extraer texto del PDF."
                };

            var datos = _pdfService.ExtraerDatosPdf(texto);
            if (datos == null)
                return new ValidacionSunatDto
                {
                    Exito       = false,
                    EstadoSunat = "ERROR",
                    Motivo      = "No se pudieron identificar los datos."
                };

            var resultado = await _sunatService.ValidarComprobanteAsync(
                numRuc:       datos.RucProveedor,
                codComp:      datos.TipoSunat,
                numeroSerie:  datos.Serie,
                numero:       datos.Numero,
                fechaEmision: datos.FechaEmision,
                monto:        datos.MontoTotal);

            if (resultado.CodigoEstado == "1" || resultado.CodigoEstado == "3")
            {
                resultado.Datos = datos;
                resultado.Folio = await GenerarFolioAsync();
            }

            return resultado;
        }

        // ── Imputaciones ──────────────────────────
        public async Task<ImputacionDetalleDto> AgregarImputacionAsync(
            AgregarImputacionCommand command)
        {
            var dto = command.Imputacion;
            _logger.LogInformation("Agregando imputación para folio {Folio}", dto.Folio);

            var maxSec = await _contexto.ImputacionesContables
                .Where(x => x.Folio == dto.Folio)
                .MaxAsync(x => (int?)x.Secuencia) ?? 0;

            var nuevaSecuencia = maxSec + 1;

            // ── Validación de monto por secuencia ─────────────────────────────
            // Seq 1 = cabecera SyteLine (no requiere monto específico).
            // Seq 2+ = deben coincidir con los montos del comprobante en este orden:
            //   2 → MontoNeto | 3 → IGV crédito fiscal | 4 → Exento | 4/5 → Retención
            if (nuevaSecuencia > 1)
            {
                var cpte = await _contexto.Comprobantes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Folio == dto.Folio);

                // El comprobante puede no estar en DB todavía (folio generado en validación
                // XML pero aún no guardado). En ese caso se omite la validación de montos;
                // la validación frontend mediante el pre-fill de secuencias es suficiente.
                if (cpte is not null)
                {
                    // Bloquear si el total ya imputado (seq > 1) coincide con MontoTotal.
                    var totalImputado = await _contexto.ImputacionesContables
                        .Where(x => x.Folio == dto.Folio && x.Secuencia > 1)
                        .SumAsync(x => (decimal?)x.Monto) ?? 0m;

                    if (Math.Abs(totalImputado - cpte.MontoTotal) <= 0.02m)
                        throw new AppException(
                            "La imputación contable está completa. El total imputado ya coincide con el monto del comprobante.",
                            "IMPUTACION_COMPLETA");

                    var lineas = LineasEsperadas(cpte);
                    var idx = nuevaSecuencia - 1; // 0-based: seq 2 → idx 1, seq 3 → idx 2 …

                    if (idx < lineas.Count)
                    {
                        var (montoEsperado, descLinea) = lineas[idx];
                        const decimal tolerancia = 0.02m; // margen por redondeo de decimales

                        if (Math.Abs(dto.Monto - montoEsperado) > tolerancia)
                            throw new AppException(
                                $"La imputación {nuevaSecuencia} debe corresponder al {descLinea} " +
                                $"({montoEsperado:N2}). Se recibió {dto.Monto:N2}.",
                                "MONTO_IMPUTACION_INVALIDO");
                    }
                }
            }

            var entidad = dto.Adapt<ImputacionContable>();
            entidad.Secuencia  = nuevaSecuencia;
            entidad.UsuarioReg = _usuario.Correo;
            entidad.FechaReg   = DateTime.Now;

            await _contexto.ImputacionesContables.AddAsync(entidad);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Imputación {Sec} agregada para folio {Folio}",
                entidad.Secuencia, dto.Folio);

            return entidad.Adapt<ImputacionDetalleDto>();
        }

        /// <summary>
        /// Construye la secuencia de montos esperados para las imputaciones de un comprobante.
        /// Índice 0 = seq 1 (cabecera, sin validación de monto).
        /// Índice 1 = seq 2 → MontoNeto.
        /// Índice 2 = seq 3 → IGV Crédito Fiscal.
        /// Índice 3 = seq 4 → MontoExento  (solo si > 0).
        /// Índice 3/4 = seq 4/5 → MontoRetencion (solo si > 0).
        /// </summary>
        private static IReadOnlyList<(decimal Monto, string Descripcion)> LineasEsperadas(
            Comprobante c)
        {
            var lista = new List<(decimal, string)>
            {
                (0m,                "cabecera SyteLine"),    // seq 1 – sin validación
                (c.MontoNeto,       "monto neto"),           // seq 2
                (c.MontoIGVCredito, "IGV crédito fiscal"),   // seq 3
            };
            if (c.MontoExento > 0)
                lista.Add((c.MontoExento,    "monto exento/exonerado")); // seq 4
            if (c.MontoRetencion > 0)
                lista.Add((c.MontoRetencion, "monto de retención"));     // seq 4 ó 5
            return lista.AsReadOnly();
        }

        public async Task<ImputacionDetalleDto> EditarImputacionAsync(
            EditarImputacionCommand command)
        {
            var dto      = command.Imputacion;
            var secuencia = int.TryParse(dto.Secuencia, out var s) ? s : 0;

            _logger.LogInformation("Editando imputación {Sec} del folio {Folio}",
                secuencia, dto.Folio);

            var entidad = await _contexto.ImputacionesContables
                .FirstOrDefaultAsync(x => x.Folio == dto.Folio
                                       && x.Secuencia == secuencia)
                ?? throw new ImputacionNotFoundException(dto.Folio, secuencia);

            entidad.AliasCuenta      = dto.AliasCuenta;
            entidad.CuentaContable   = dto.CuentaContable;
            entidad.DescripcionCuenta = dto.DescripcionCuenta;
            entidad.Monto            = dto.Monto;
            entidad.Descripcion      = dto.Descripcion;
            entidad.Proyecto         = dto.Proyecto;
            entidad.CodUnidad1Cuenta = dto.CodUnidad1Cuenta;
            entidad.CodUnidad3Cuenta = dto.CodUnidad3Cuenta;
            entidad.CodUnidad4Cuenta = dto.CodUnidad4Cuenta;
            entidad.UsuarioAct       = _usuario.Correo;
            entidad.FechaAct         = DateTime.Now;

            await _unitOfWork.SaveChangesAsync();
            return entidad.Adapt<ImputacionDetalleDto>();
        }

        public async Task EliminarImputacionAsync(EliminarImputacionCommand command)
        {
            var entidad = await _contexto.ImputacionesContables
                .FirstOrDefaultAsync(x => x.Folio == command.Folio
                                       && x.Secuencia == command.Secuencia);
            if (entidad != null)
            {
                _contexto.ImputacionesContables.Remove(entidad);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Imputación {Sec} eliminada del folio {Folio}",
                    command.Secuencia, command.Folio);
            }
        }

        public Task<IEnumerable<ImputacionDetalleDto>> CargarImputacionMasivaAsync(
            IFormFile file)
            => Task.FromResult<IEnumerable<ImputacionDetalleDto>>(new List<ImputacionDetalleDto>());

        // ── Validar ZIP SUNAT ─────────────────────
        public async Task<ValidacionSunatDto> ValidarZipSunatAsync(IFormFile archivo)
        {
            using var zipStream = archivo.OpenReadStream();
            using var zip = new ZipArchive(zipStream, ZipArchiveMode.Read);

            var entradaXml = zip.Entries.FirstOrDefault(e =>
                e.Name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase) &&
                !e.Name.StartsWith("R-", StringComparison.OrdinalIgnoreCase));

            if (entradaXml == null)
                return new ValidacionSunatDto
                {
                    Exito       = false,
                    EstadoSunat = "ERROR",
                    Motivo      = "El ZIP no contiene un archivo XML de comprobante."
                };

            using var xmlStream = entradaXml.Open();
            var datos = _xmlService.LeerDatosXml(xmlStream);

            var resultado = await _sunatService.ValidarComprobanteAsync(
                numRuc:       datos.RucProveedor,
                codComp:      datos.TipoSunat,
                numeroSerie:  datos.Serie,
                numero:       datos.Numero,
                fechaEmision: datos.FechaEmision,
                monto:        datos.MontoTotal);

            if (resultado.CodigoEstado == "1" || resultado.CodigoEstado == "3")
            {
                resultado.Datos = datos;
                resultado.Folio = await GenerarFolioAsync();

                var archivos = new List<(byte[] contenido, string nombre, string tipo, string subTipo)>();
                foreach (var entrada in zip.Entries)
                {
                    var ext    = Path.GetExtension(entrada.Name).TrimStart('.').ToLowerInvariant();
                    var esCdr  = entrada.Name.StartsWith("R-", StringComparison.OrdinalIgnoreCase);

                    // Incluir: xml, pdf, y zip solo si es CDR (R-*)
                    if (ext != "xml" && ext != "pdf" && !(esCdr && ext == "zip")) continue;

                    using var ms = new MemoryStream();
                    using var entStream = entrada.Open();
                    await entStream.CopyToAsync(ms);

                    string tipo, subTipo;
                    if (ext == "pdf")              { tipo = "PDF"; subTipo = "REPRESENTACION_IMPRESA"; }
                    else if (esCdr && ext == "zip"){ tipo = "ZIP"; subTipo = "XML_CDR"; }
                    else if (esCdr)                { tipo = "XML"; subTipo = "XML_CDR"; }
                    else                           { tipo = "XML"; subTipo = "XML_SUNAT"; }
                    archivos.Add((ms.ToArray(), entrada.Name, tipo, subTipo));
                }
                await GuardarDocumentosAsync(resultado.Folio, archivos);
            }

            return resultado;
        }

        // ── Guardar Documentos Electrónicos ──────
        public async Task GuardarDocumentosAsync(
            string folio,
            List<(byte[] contenido, string nombre, string tipo, string subTipo)> archivos)
        {
            foreach (var (contenido, nombre, tipo, subTipo) in archivos)
            {
                var doc = new DocumentoElectronico
                {
                    Folio         = folio,
                    TipoArchivo   = tipo,
                    SubTipo       = subTipo,
                    NombreArchivo = nombre,
                    Contenido     = contenido,
                    FechaReg      = DateTime.Now,
                    UsuarioReg    = _usuario.Correo
                };
                await _contexto.DocumentosElectronicos.AddAsync(doc);
            }
            await _unitOfWork.SaveChangesAsync();
        }

        // ── Descargar Documento Electrónico ───────
        public async Task<DocumentoElectronico?> DescargarDocumentoAsync(int idDocumento)
            => await _contexto.DocumentosElectronicos
                .FirstOrDefaultAsync(x => x.IdDocumento == idDocumento);

        // ── Eliminar Documento Electrónico ────────
        public async Task EliminarDocumentoAsync(int idDocumento)
        {
            var doc = await _contexto.DocumentosElectronicos
                .FirstOrDefaultAsync(x => x.IdDocumento == idDocumento);
            if (doc != null)
            {
                _contexto.DocumentosElectronicos.Remove(doc);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        // ── Helpers ───────────────────────────────
        private async Task<Comprobante> ObtenerPorFolioAsync(string folio)
            => await _contexto.Comprobantes
                .FirstOrDefaultAsync(x => x.Folio == folio)
                ?? throw new ComprobanteNotFoundException(folio);

        private static DateTime ParseFecha(string? fecha)
            => DateTime.TryParse(fecha, out var result) ? result : DateTime.Now;

        private static DateTime? ParseFechaNullable(string? fecha)
        {
            if (string.IsNullOrEmpty(fecha)) return null;
            return DateTime.TryParse(fecha, out var result) ? result : null;
        }
    }
}
