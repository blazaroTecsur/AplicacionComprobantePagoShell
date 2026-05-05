using ComprobantePago.Application.DTOs.Comprobante.Requests;
using ComprobantePago.Application.DTOs.Comprobante.Response;
using ComprobantePago.Application.DTOs.Responses;
using ComprobantePago.Application.Interfaces.QueryServices;
using ComprobantePago.Application.Settings;
using ComprobantePago.Infrastructure.Persistence;
using ComprobantePago.Infrastructure.Services;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ComprobantePago.Infrastructure.QueryServices
{
    public class ComprobanteQueryService(
        AppDbContext contexto,
        ILogger<ComprobanteQueryService> logger,
        IOptions<EmpresaSettings> empresaOptions)
        : IComprobanteQueryService
    {
        private readonly AppDbContext _contexto = contexto;
        private readonly ILogger<ComprobanteQueryService> _logger = logger;
        private readonly EmpresaSettings _empresa = empresaOptions.Value;

        // ── Buscar comprobantes ───────────────────
        public async Task<IEnumerable<ComprobanteDto>> BuscarAsync(
            BuscarComprobanteDto filtros)
        {
            _logger.LogInformation("Buscando comprobantes con filtros: {@Filtros}", filtros);
            var query = _contexto.Comprobantes.AsQueryable();

            if (!string.IsNullOrEmpty(filtros.Tipo))
                query = query.Where(x => x.TipoDocumento == filtros.Tipo);

            if (!string.IsNullOrEmpty(filtros.Estado))
                query = query.Where(x => x.CodigoEstado == filtros.Estado);

            if (!string.IsNullOrEmpty(filtros.Proveedor))
                query = query.Where(x =>
                    x.RucReceptor.Contains(filtros.Proveedor) ||
                    x.RazonSocialReceptor.Contains(filtros.Proveedor));

            if (!string.IsNullOrEmpty(filtros.Folio))
                query = query.Where(x => x.Folio.Contains(filtros.Folio));

            return await query
                .OrderByDescending(x => x.FechaReg)
                .Select(x => new ComprobanteDto
                {
                    Folio = x.Folio,
                    TipoComprobante = x.TipoDocumento,
                    Serie = x.Serie,
                    Numero = x.Numero,
                    Proveedor = x.RazonSocialReceptor,
                    Fecha = x.FechaEmision.ToString("dd/MM/yyyy"),
                    Moneda = x.Moneda,
                    MontoTotal = x.MontoTotal,
                    Estado = x.CodigoEstado,
                    VoucherSyteline = x.VoucherSyteline
                })
                .ToListAsync();
        }

        // ── Obtener detalle ───────────────────────
        public async Task<ComprobanteDetalleDto> ObtenerDetalleAsync(string folio)
        {
            var c = await _contexto.Comprobantes
                .FirstOrDefaultAsync(x => x.Folio == folio);

            if (c == null) return null!;

            return new ComprobanteDetalleDto
            {
                Folio = c.Folio,
                Ruc = c.RucReceptor,
                RazonSocial = c.RazonSocialReceptor,
                TipoDocumento = c.TipoDocumento,
                TipoSunat = c.TipoSunat,
                Serie = c.Serie,
                Numero = c.Numero,
                FechaEmision = c.FechaEmision.ToString("yyyy-MM-dd"),
                FechaRecepcion = c.FechaRecepcion?.ToString("yyyy-MM-dd"),
                Moneda = c.Moneda,
                TasaCambio = c.TasaCambio,
                LugarPago = c.LugarPago,
                PlazoPago = c.PlazoPago,
                FechaVencimiento = c.FechaVencimiento?.ToString("yyyy-MM-dd"),
                RucBenef = c.RucBeneficiario,
                RazonSocialBenef = c.RazonSocialBenef,
                Observacion = c.Observacion,
                OrdenCompra = c.OrdenCompra,
                FactMultiple = c.FactMultiple,
                TieneDetraccion = c.TieneDetraccion,
                TipoDetraccion = c.TipoDetraccion,
                PorcentajeDetraccion = c.PorcentajeDetraccion ?? 0,
                ConstanciaDeposito = c.ConstanciaDeposito,
                FechaDeposito = c.FechaDeposito?.ToString("yyyy-MM-dd"),
                MontoNeto = c.MontoNeto,
                MontoExento = c.MontoExento,
                MontoIGVCosto = c.MontoIGVCosto,
                PorcentajeIGV = c.PorcentajeIGV,
                MontoIGVCredito = c.MontoIGVCredito,
                MontoTotal = c.MontoTotal,
                MontoBruto = c.MontoBruto,
                MontoRetencion = c.MontoRetencion,
                MontoDetraccion = c.MontoDetraccion,
                CodigoEstado = c.CodigoEstado,
                Estado = c.CodigoEstado,
                RolDigitacion    = c.RolDigitacion,
                FechaDigitacion  = c.FechaDigitacion?.ToString("dd/MM/yyyy HH:mm"),
                RolAutorizacion  = c.RolAutorizacion,
                FechaAutorizacion = c.FechaAutorizacion?.ToString("dd/MM/yyyy HH:mm"),
                RolAprobacion    = c.RolAprobacion,
                FechaAprobacion  = c.FechaAprobacion?.ToString("dd/MM/yyyy HH:mm"),
                RolAnulacion     = c.RolAnulacion,
                FechaAnulacion   = c.FechaAnulacion?.ToString("dd/MM/yyyy HH:mm"),
                Mensaje = c.Mensaje,
                EsEmpleado     = c.EsEmpleado,
                EmpleadoCodigo = c.EmpleadoCodigo ?? string.Empty,
                EmpleadoNombre = c.EmpleadoNombre ?? string.Empty,
                RequiereAduana = c.RequiereAduana,
                RequiereDetraccion = c.RequiereDetraccion,
                EsDocumentoElectronico = c.EsDocumentoElectronico ? "S" : "N"
            };
        }

        public async Task<byte[]?> ObtenerPdfAsync(string folio)
        {
            var c = await _contexto.Comprobantes
                .FirstOrDefaultAsync(x => x.Folio == folio);
            if (c is null) return null;

            var imputaciones = await _contexto.ImputacionesContables
                .Where(x => x.Folio == folio)
                .OrderBy(x => x.Secuencia)
                .ToListAsync();

            if (!imputaciones.Any()) return null;

            // Descripción del tipo de documento
            var descTipo = await _contexto.TiposDocumento
                .Where(t => t.Codigo == c.TipoDocumento)
                .Select(t => t.Descripcion)
                .FirstOrDefaultAsync() ?? string.Empty;

            // Descripción del lugar de pago
            var descLugar = await _contexto.LugaresPago
                .Where(l => l.Codigo == c.LugarPago)
                .Select(l => l.Descripcion)
                .FirstOrDefaultAsync() ?? string.Empty;

            var data = new ComprobanteReporteData
            {
                EmpresaNombre = _empresa.Nombre,
                EmpresaRuc    = _empresa.Ruc,

                RucProveedor      = c.RucReceptor,
                RazonSocial       = c.RazonSocialReceptor,
                Serie             = c.Serie,
                Numero            = c.Numero,
                FechaEmision      = c.FechaEmision.ToString("dd/MM/yyyy"),
                TipoDocumento     = c.TipoDocumento,
                TipoSunat         = c.TipoSunat,
                DescTipoDocumento = descTipo,
                Moneda            = c.Moneda,
                TasaCambio        = c.TasaCambio,
                FechaRecepcion    = c.FechaRecepcion?.ToString("dd/MM/yyyy") ?? string.Empty,
                LugarPago         = c.LugarPago ?? string.Empty,
                DescLugarPago     = descLugar,
                RucBenef          = c.RucBeneficiario ?? string.Empty,
                OrdenCompra       = c.OrdenCompra ?? string.Empty,
                FechaVencimiento  = c.FechaVencimiento?.ToString("dd/MM/yyyy") ?? string.Empty,
                EsDocumentoElectronico = c.EsDocumentoElectronico,
                EstadoSunat       = c.EstadoSunat ?? string.Empty,
                CodigoEstado      = c.CodigoEstado,

                MontoNeto        = c.MontoNeto,
                MontoExento      = c.MontoExento,
                PorcentajeIGV    = c.PorcentajeIGV,
                MontoIGVCredito  = c.MontoIGVCredito,
                MontoBruto       = c.MontoBruto,
                MontoRetencion   = c.MontoRetencion,
                MontoDetraccion  = c.MontoDetraccion,

                TieneDetraccion      = c.TieneDetraccion,
                TipoDetraccion       = c.TipoDetraccion ?? string.Empty,
                PorcentajeDetraccion = c.PorcentajeDetraccion ?? 0,

                RolDigitacion   = c.RolDigitacion ?? string.Empty,
                FechaDigitacion = c.FechaDigitacion?.ToString("dd/MM/yyyy") ?? string.Empty,
                RolAutorizacion = c.RolAutorizacion ?? string.Empty,
                Observacion     = c.Observacion ?? string.Empty,

                Imputaciones = imputaciones.Select(i => new ImputacionReporteData
                {
                    CuentaContable   = i.CuentaContable ?? string.Empty,
                    CodUnidad1Cuenta = i.CodUnidad1Cuenta ?? string.Empty,
                    CodUnidad3Cuenta = i.CodUnidad3Cuenta ?? string.Empty,
                    CodUnidad4Cuenta = i.CodUnidad4Cuenta ?? string.Empty,
                    Monto            = i.Monto,
                    Descripcion      = i.Descripcion ?? string.Empty
                }).ToList()
            };

            return ComprobantePdfReporteService.Generar(data);
        }

        public async Task<IEnumerable<DocumentoElectronicoDto>>
            ObtenerDocumentosElectronicosAsync(string folio)
        {
            return await _contexto.DocumentosElectronicos
                .Where(x => x.Folio == folio)
                .OrderBy(x => x.FechaReg)
                .Select(x => new DocumentoElectronicoDto
                {
                    IdDocumento = x.IdDocumento,
                    TipoArchivo = x.TipoArchivo,
                    SubTipo = x.SubTipo,
                    NombreArchivo = x.NombreArchivo,
                    FechaReg = x.FechaReg.ToString("dd/MM/yyyy HH:mm"),
                    TamanioBytes = x.Contenido.Length
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<ImputacionDetalleDto>>
            ObtenerImputacionesAsync(string folio)
        {
            _logger.LogInformation("Obteniendo imputaciones para folio {Folio}", folio);

            var lista = await _contexto.ImputacionesContables
                .Where(x => x.Folio == folio)
                .OrderBy(x => x.Secuencia)
                .ToListAsync();

            return lista.Adapt<IEnumerable<ImputacionDetalleDto>>();
        }

        public Task<byte[]> ObtenerPlantillaImputacionAsync()
            => Task.FromResult<byte[]>(null!);
    }
}
