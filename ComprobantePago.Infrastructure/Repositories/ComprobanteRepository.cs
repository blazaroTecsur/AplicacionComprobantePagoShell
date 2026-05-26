using ClosedXML.Excel;
using ComprobantePago.Application.Commands.Comprobante;
using ComprobantePago.Application.Commands.Imputacion;
using ComprobantePago.Application.DTOs.Comprobante.Requests;
using ComprobantePago.Application.DTOs.Comprobante.Response;
using ComprobantePago.Application.Exceptions;
using ComprobantePago.Application.Interfaces;
using ComprobantePago.Application.Interfaces.Repositories;
using ComprobantePago.Application.Interfaces.Services;
using Seguridad.Abstractions.Interfaces;
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

        // ── Generar Serie + Número (PV / VC / PT) ────────────────────
        public async Task<(string serie, string numero)> GenerarSerieNumeroAsync(string tipoDocumento)
        {
            var ahora   = DateTime.Now;
            var anio    = ahora.Year;
            var mes     = ahora.Month;
            var empresa = _usuario.Empresa ?? string.Empty;

            var registro = await _contexto.SeriesCorrelativo
                .FirstOrDefaultAsync(x => x.CodigoEmpresa == empresa
                                       && x.TipoDocumento  == tipoDocumento
                                       && x.Anio           == anio
                                       && x.Mes            == mes);

            if (registro is null)
            {
                registro = new Domain.Entities.SerieCorrelativo
                {
                    CodigoEmpresa      = empresa,
                    TipoDocumento      = tipoDocumento,
                    Anio               = anio,
                    Mes                = mes,
                    UltimoCorrelativo  = 1
                };
                await _contexto.SeriesCorrelativo.AddAsync(registro);
            }
            else
            {
                registro.UltimoCorrelativo++;
            }

            await _unitOfWork.SaveChangesAsync();

            var serie  = tipoDocumento; // "PV", "VC" o "PT"
            var numero = $"{anio:D4}{mes:D2}{registro.UltimoCorrelativo:D5}";
            return (serie, numero);
        }

        // ── Guardar Comprobante ───────────────────
        public async Task<(string folio, string serie, string numero)> GuardarAsync(RegistrarComprobanteCommand command)
        {
            var dto = command.Comprobante;
            var folio = string.IsNullOrWhiteSpace(dto.Folio)
                ? await GenerarFolioAsync()
                : dto.Folio;

            // Para PV/VC/PT nuevos sin número: generar serie+número en el momento del guardado
            var serieEfectiva  = dto.Serie;
            var numeroEfectivo = dto.Numero;
            var tiposAutoNumero = new[] { "PV", "VC", "PT" };
            if (string.IsNullOrWhiteSpace(dto.Folio) &&
                tiposAutoNumero.Contains(dto.TipoDocumento) &&
                string.IsNullOrWhiteSpace(dto.Numero))
            {
                (serieEfectiva, numeroEfectivo) = await GenerarSerieNumeroAsync(dto.TipoDocumento);
            }

            var duplicado = await _contexto.Comprobantes
                .AnyAsync(x => x.Folio        != folio
                            && x.RucReceptor  == dto.Ruc
                            && x.Serie        == serieEfectiva
                            && x.Numero       == numeroEfectivo
                            && x.CodigoEstado != "ANULADO");

            if (duplicado)
                throw new InvalidOperationException(
                    $"Ya existe un comprobante con serie {serieEfectiva}-{numeroEfectivo} para el RUC {dto.Ruc}.");
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
                    existente.Serie                = serieEfectiva;
                    existente.Numero               = numeroEfectivo;
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
                        Serie                  = serieEfectiva,
                        Numero                 = numeroEfectivo,
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
                        FechaReg               = DateTime.Now,
                        CodigoEmpresa          = _usuario.Empresa
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

            return (folio, serieEfectiva, numeroEfectivo);
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

        // ── Firmar masivo (Autorizador) ───────────
        public async Task FirmarMasivoAsync(IEnumerable<string> folios, string usuarioCorreo)
        {
            var lista = folios.ToList();
            var comprobantes = await _contexto.Comprobantes
                .Where(x => lista.Contains(x.Folio) && x.CodigoEstado == "ENVIADO")
                .ToListAsync();

            foreach (var c in comprobantes)
            {
                c.CodigoEstado      = "AUTORIZADO";
                c.RolAutorizacion   = usuarioCorreo;
                c.FechaAutorizacion = DateTime.Now;
                c.UsuarioAct        = usuarioCorreo;
                c.FechaAct          = DateTime.Now;
            }
            await _unitOfWork.SaveChangesAsync();
        }

        // ── Aprobar masivo (Aprobador) ────────────
        public async Task AprobarMasivoAsync(IEnumerable<string> folios, string usuarioCorreo)
        {
            var lista = folios.ToList();
            var comprobantes = await _contexto.Comprobantes
                .Where(x => lista.Contains(x.Folio) && x.CodigoEstado == "AUTORIZADO")
                .ToListAsync();

            foreach (var c in comprobantes)
            {
                c.CodigoEstado    = "APROBADO";
                c.RolAprobacion   = usuarioCorreo;
                c.FechaAprobacion = DateTime.Now;
                c.UsuarioAct      = usuarioCorreo;
                c.FechaAct        = DateTime.Now;
            }
            await _unitOfWork.SaveChangesAsync();
        }

        // ── Pagar ─────────────────────────────────
        public async Task PagarAsync(string folio, string usuarioCorreo)
        {
            var c = await ObtenerPorFolioAsync(folio);
            c.CodigoEstado = "PAGADO";
            c.UsuarioAct   = usuarioCorreo;
            c.FechaAct     = DateTime.Now;
            await _unitOfWork.SaveChangesAsync();
        }

        // ── Anular ────────────────────────────────
        public async Task AnularAsync(AnularComprobanteCommand command)
        {
            var c = await ObtenerPorFolioAsync(command.Comprobante.Folio);
            if (c.CodigoEstado != "AUTORIZADO")
                throw new InvalidOperationException(
                    $"Solo se puede anular un comprobante en estado AUTORIZADO. Estado actual: {c.CodigoEstado}.");
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

            // Si el frontend solicita explícitamente seq=1 (cabecera SyteLine) y no existe
            // todavía (ej: carga masiva insertó las líneas de detalle sin la cabecera),
            // respetamos esa secuencia y omitimos la validación de montos.
            bool esCabeceraExplicita = int.TryParse(dto.Secuencia, out int seqSolicitada)
                                       && seqSolicitada == 1
                                       && maxSec >= 1; // ya hay líneas pero falta la cabecera

            if (esCabeceraExplicita)
            {
                var existe = await _contexto.ImputacionesContables
                    .AnyAsync(x => x.Folio == dto.Folio && x.Secuencia == 1);
                if (existe)
                    throw new AppException(
                        "La cabecera SyteLine ya existe para este comprobante.",
                        "CABECERA_DUPLICADA");
            }

            var nuevaSecuencia = esCabeceraExplicita ? 1 : maxSec + 1;

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
        /// Facturación solo exenta (neto=0, igv=0): seq 1 Cabecera + seq 2 Exento (2 líneas).
        /// Caso normal: seq 1 Cabecera + seq 2 Neto + seq 3 IGV [+ seq 4 Exento] [+ seq 4/5 Retención].
        /// </summary>
        private static IReadOnlyList<(decimal Monto, string Descripcion)> LineasEsperadas(
            Comprobante c)
        {
            var lista = new List<(decimal, string)>
            {
                (0m, "cabecera SyteLine"), // seq 1 – sin validación
            };

            bool soloExento = c.MontoNeto == 0m && c.MontoIGVCredito == 0m && c.MontoExento > 0m;
            if (soloExento)
            {
                lista.Add((c.MontoExento, "monto exento/exonerado")); // seq 2
            }
            else
            {
                lista.Add((c.MontoNeto,       "monto neto"));           // seq 2
                lista.Add((c.MontoIGVCredito, "IGV crédito fiscal"));   // seq 3
                if (c.MontoExento > 0)
                    lista.Add((c.MontoExento,    "monto exento/exonerado")); // seq 4
                if (c.MontoRetencion > 0)
                    lista.Add((c.MontoRetencion, "monto de retención"));     // seq 4 ó 5
            }

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
            entidad.CodUnidad2Cuenta = dto.CodUnidad2Cuenta;
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

        public async Task<IEnumerable<ImputacionDetalleDto>> CargarImputacionMasivaAsync(
            IFormFile file, string folio)
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            ms.Position = 0;

            using var workbook = new XLWorkbook(ms);
            var hoja = workbook.Worksheets.First();

            // Columnas según plantilla:
            // 1=Secuencia 2=AliasCuenta 3=CuentaContable 4=DescripcionCuenta
            // 5=Monto 6=TipoLinea 7=Descripcion 8=Proyecto
            // 9=CodUnidad1 10=CodUnidad2 11=CodUnidad3 12=CodUnidad4

            var lineasExcel = new List<ImputacionContable>();
            var numerosFilaExcel = new List<int>(); // nro de fila Excel por cada entrada en lineasExcel

            foreach (var fila in hoja.RowsUsed().Skip(1)) // fila 1 = encabezados
            {
                string GetCell(int col) => fila.Cell(col).GetValue<string>()?.Trim() ?? string.Empty;
                decimal GetDecimal(int col)
                {
                    var val = fila.Cell(col).GetValue<string>();
                    return decimal.TryParse(val,
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out var d) ? d : 0m;
                }

                var alias    = GetCell(2);
                var tipoLinea = GetCell(6).ToUpperInvariant();

                // Ignorar filas vacías o la fila de cabecera (no se importa)
                if (string.IsNullOrWhiteSpace(alias)) continue;
                if (tipoLinea == "CABECERA") continue;

                numerosFilaExcel.Add(fila.RowNumber());
                lineasExcel.Add(new ImputacionContable
                {
                    Folio             = folio,
                    AliasCuenta       = alias,
                    CuentaContable    = GetCell(3),
                    DescripcionCuenta = GetCell(4),
                    Monto             = GetDecimal(5),
                    TipoLinea         = string.IsNullOrEmpty(tipoLinea) ? null : tipoLinea,
                    Descripcion       = GetCell(7),
                    Proyecto          = GetCell(8),
                    CodUnidad1Cuenta  = GetCell(9),
                    CodUnidad2Cuenta  = GetCell(10),
                    CodUnidad3Cuenta  = GetCell(11),
                    CodUnidad4Cuenta  = GetCell(12),
                    UsuarioReg        = _usuario.Correo,
                    FechaReg          = DateTime.Now
                });
            }

            if (lineasExcel.Count == 0)
                throw new InvalidOperationException("El archivo no contiene líneas de imputación válidas.");

            // ── Validar catálogo de cuentas contables y códigos de unidad ─────
            var empresa = _usuario.Empresa;

            var cuentasValidas = (await _contexto.CuentasContables
                .Where(x => x.Activo && x.Codigo.Length >= 7)
                .Select(x => x.Codigo)
                .ToListAsync()).ToHashSet();

            var unidad1Validas = (await _contexto.CodigosUnidad1
                .Where(x => x.Activo && (string.IsNullOrEmpty(empresa) || x.Empresa == empresa))
                .Select(x => x.Codigo)
                .ToListAsync()).ToHashSet();

            var unidad2Validas = (await _contexto.CodigosUnidad2
                .Where(x => x.Activo && (string.IsNullOrEmpty(empresa) || x.Empresa == empresa))
                .Select(x => x.Codigo)
                .ToListAsync()).ToHashSet();

            var unidad3Validas = (await _contexto.CodigosUnidad3
                .Where(x => x.Activo && (string.IsNullOrEmpty(empresa) || x.Empresa == empresa))
                .Select(x => x.Codigo)
                .ToListAsync()).ToHashSet();

            var unidad4Validas = (await _contexto.CodigosUnidad4
                .Where(x => x.Activo)
                .Select(x => x.Codigo)
                .ToListAsync()).ToHashSet();

            var erroresCatalogo = new List<string>();
            for (int i = 0; i < lineasExcel.Count; i++)
            {
                var linea   = lineasExcel[i];
                var nroFila = numerosFilaExcel[i];
                var prefijo = $"Fila {nroFila}";

                if (string.IsNullOrWhiteSpace(linea.CuentaContable))
                    erroresCatalogo.Add($"{prefijo}: la cuenta contable es obligatoria.");
                else if (linea.CuentaContable.Length < 7)
                    erroresCatalogo.Add($"{prefijo}: cuenta '{linea.CuentaContable}' tiene menos de 7 dígitos.");
                else if (!cuentasValidas.Contains(linea.CuentaContable))
                    erroresCatalogo.Add($"{prefijo}: cuenta '{linea.CuentaContable}' no existe en el catálogo.");

                // Cuentas que inician con 6: unidad 1 y unidad 4 son obligatorias
                if (!string.IsNullOrWhiteSpace(linea.CuentaContable) && linea.CuentaContable.StartsWith("6"))
                {
                    if (string.IsNullOrWhiteSpace(linea.CodUnidad1Cuenta))
                        erroresCatalogo.Add($"{prefijo}: cuenta '{linea.CuentaContable}' inicia con 6, el código de unidad 1 es obligatorio.");
                    if (string.IsNullOrWhiteSpace(linea.CodUnidad4Cuenta))
                        erroresCatalogo.Add($"{prefijo}: cuenta '{linea.CuentaContable}' inicia con 6, el código de unidad 4 es obligatorio.");
                }

                if (!string.IsNullOrWhiteSpace(linea.CodUnidad1Cuenta) &&
                    !unidad1Validas.Contains(linea.CodUnidad1Cuenta))
                    erroresCatalogo.Add($"{prefijo}: código unidad 1 '{linea.CodUnidad1Cuenta}' no existe en el catálogo.");

                if (!string.IsNullOrWhiteSpace(linea.CodUnidad2Cuenta) &&
                    !unidad2Validas.Contains(linea.CodUnidad2Cuenta))
                    erroresCatalogo.Add($"{prefijo}: código unidad 2 '{linea.CodUnidad2Cuenta}' no existe en el catálogo.");

                if (!string.IsNullOrWhiteSpace(linea.CodUnidad3Cuenta) &&
                    !unidad3Validas.Contains(linea.CodUnidad3Cuenta))
                    erroresCatalogo.Add($"{prefijo}: código unidad 3 '{linea.CodUnidad3Cuenta}' no existe en el catálogo.");

                if (!string.IsNullOrWhiteSpace(linea.CodUnidad4Cuenta) &&
                    !unidad4Validas.Contains(linea.CodUnidad4Cuenta))
                    erroresCatalogo.Add($"{prefijo}: código unidad 4 '{linea.CodUnidad4Cuenta}' no existe en el catálogo.");
            }

            if (erroresCatalogo.Count > 0)
                throw new InvalidOperationException(
                    "La plantilla contiene errores de catálogo:\n" +
                    string.Join("\n", erroresCatalogo));

            // Validar que los totales del Excel no superen los montos de cabecera
            var comprobante = await _contexto.Comprobantes.FirstOrDefaultAsync(x => x.Folio == folio)
                ?? throw new InvalidOperationException($"Comprobante {folio} no encontrado.");

            var totalGravado = lineasExcel.Where(l => l.TipoLinea == "GRAVADO").Sum(l => l.Monto);
            var totalExento  = lineasExcel.Where(l => l.TipoLinea == "EXENTO").Sum(l => l.Monto);

            if (comprobante.MontoNeto > 0 && Math.Abs(totalGravado - comprobante.MontoNeto) > 0.02m)
                throw new InvalidOperationException(
                    $"El total de líneas GRAVADO ({totalGravado:N2}) debe coincidir con el monto neto del comprobante ({comprobante.MontoNeto:N2}).");

            if (comprobante.MontoExento > 0 && Math.Abs(totalExento - comprobante.MontoExento) > 0.02m)
                throw new InvalidOperationException(
                    $"El total de líneas EXENTO ({totalExento:N2}) debe coincidir con el monto exonerado del comprobante ({comprobante.MontoExento:N2}).");

            // Eliminar imputaciones existentes con secuencia > 1 y reemplazar
            var existentes = await _contexto.ImputacionesContables
                .Where(x => x.Folio == folio && x.Secuencia > 1)
                .ToListAsync();
            _contexto.ImputacionesContables.RemoveRange(existentes);

            int seq = 2;
            foreach (var linea in lineasExcel)
                linea.Secuencia = seq++;

            _contexto.ImputacionesContables.AddRange(lineasExcel);
            await _unitOfWork.SaveChangesAsync();

            var todas = await _contexto.ImputacionesContables
                .Where(x => x.Folio == folio)
                .OrderBy(x => x.Secuencia)
                .ToListAsync();

            return todas.Adapt<IEnumerable<ImputacionDetalleDto>>();
        }

        public async Task<IEnumerable<ImputacionDetalleDto>> FraccionarImputacionAsync(
            string folio,
            List<ImputacionFraccionDto> lineasGravado,
            List<ImputacionFraccionDto> lineasExento)
        {
            var comprobante = await _contexto.Comprobantes
                .FirstOrDefaultAsync(x => x.Folio == folio)
                ?? throw new InvalidOperationException($"Comprobante {folio} no encontrado.");

            var existentes = await _contexto.ImputacionesContables
                .Where(x => x.Folio == folio && x.Secuencia > 1)
                .ToListAsync();
            _contexto.ImputacionesContables.RemoveRange(existentes);

            var nuevas = new List<ImputacionContable>();
            int seq = 2;

            foreach (var linea in lineasGravado)
            {
                nuevas.Add(new ImputacionContable
                {
                    Folio       = folio,
                    Secuencia   = seq++,
                    TipoLinea   = "GRAVADO",
                    Monto       = linea.Monto,
                    Descripcion = "Monto Neto",
                    UsuarioReg  = _usuario.Correo,
                    FechaReg    = DateTime.Now
                });
            }

            if (comprobante.MontoIGVCredito > 0)
            {
                nuevas.Add(new ImputacionContable
                {
                    Folio       = folio,
                    Secuencia   = seq++,
                    TipoLinea   = "IGV",
                    Monto       = comprobante.MontoIGVCredito,
                    Descripcion = "IGV Crédito Fiscal",
                    UsuarioReg  = _usuario.Correo,
                    FechaReg    = DateTime.Now
                });
            }

            foreach (var linea in lineasExento)
            {
                nuevas.Add(new ImputacionContable
                {
                    Folio       = folio,
                    Secuencia   = seq++,
                    TipoLinea   = "EXENTO",
                    Monto       = linea.Monto,
                    Descripcion = "Monto Exento",
                    UsuarioReg  = _usuario.Correo,
                    FechaReg    = DateTime.Now
                });
            }

            _contexto.ImputacionesContables.AddRange(nuevas);
            await _unitOfWork.SaveChangesAsync();

            var todas = await _contexto.ImputacionesContables
                .Where(x => x.Folio == folio)
                .OrderBy(x => x.Secuencia)
                .ToListAsync();

            return todas.Adapt<IEnumerable<ImputacionDetalleDto>>();
        }

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
