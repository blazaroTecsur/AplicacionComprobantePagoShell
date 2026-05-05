using ComprobantePago.Application.DTOs.Responses;
using ComprobantePago.Application.Interfaces.QueryServices;
using ComprobantePago.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ComprobantePago.Infrastructure.QueryServices
{
    public class SytelineQueryService(
        AppDbContext contexto,
        ILogger<SytelineQueryService> logger)
        : ISytelineQueryService
    {
        private readonly AppDbContext _contexto = contexto;
        private readonly ILogger<SytelineQueryService> _logger = logger;

        public async Task<IEnumerable<SytelineCabeceraDto>>
            ObtenerCabecerasSytelineAsync(List<string>? folios = null)
        {
            var query = _contexto.Comprobantes
                .Where(x => x.CodigoEstado == "APROBADO");

            if (folios != null && folios.Any())
                query = query.Where(x => folios.Contains(x.Folio));

            var comprobantes = await query
                .OrderBy(x => x.FechaAprobacion)
                .ToListAsync();

            // Índice RUC → IdProveedorExternal para resolver VendNum de proveedores
            var rucs = comprobantes
                .Where(c => !c.EsEmpleado)
                .Select(c => c.RucReceptor)
                .Distinct()
                .ToList();

            var vendNums = await _contexto.Proveedores
                .Where(p => rucs.Contains(p.Ruc))
                .ToDictionaryAsync(p => p.Ruc, p => p.IdProveedorExternal.ToString());

            // Índice Codigo → IdEmpleadoExternal para resolver VendNum de empleados
            var empleadoCodigos = comprobantes
                .Where(c => c.EsEmpleado && c.EmpleadoCodigo != null)
                .Select(c => c.EmpleadoCodigo!)
                .Distinct()
                .ToList();

            var empleadoExternals = empleadoCodigos.Count > 0
                ? await _contexto.Empleados
                    .Where(e => empleadoCodigos.Contains(e.Codigo)
                             && !string.IsNullOrEmpty(e.IdEmpleadoExternal))
                    .ToDictionaryAsync(e => e.Codigo, e => e.IdEmpleadoExternal)
                : new Dictionary<string, string>();

            var result = new List<SytelineCabeceraDto>();

            foreach (var c in comprobantes)
            {
                var primeraImp = await _contexto.ImputacionesContables
                    .Where(i => i.Folio == c.Folio)
                    .OrderBy(i => i.Secuencia)
                    .FirstOrDefaultAsync();

                var finMes = new DateTime(
                    c.FechaEmision.Year,
                    c.FechaEmision.Month,
                    DateTime.DaysInMonth(
                        c.FechaEmision.Year,
                        c.FechaEmision.Month));

                // VendNum: empleados → IdEmpleadoExternal de tmaempleado;
                // proveedores → IdProveedorExternal de tmaproveedor (0 = no sincronizado,
                // se devuelve vacío para que el envío falle con mensaje claro).
                var vendNum = c.EsEmpleado
                    ? (c.EmpleadoCodigo != null &&
                       empleadoExternals.TryGetValue(c.EmpleadoCodigo, out var extId)
                           ? extId : string.Empty)
                    : vendNums.TryGetValue(c.RucReceptor, out var vn) && vn != "0"
                        ? vn
                        : string.Empty;

                result.Add(new SytelineCabeceraDto
                {
                    Proveedor = c.EsEmpleado ? (c.EmpleadoCodigo ?? string.Empty) : c.RucReceptor,
                    VendNum   = vendNum,
                    Nombre    = c.EsEmpleado ? (c.EmpleadoNombre ?? string.Empty) : c.RazonSocialReceptor,
                    Comprobante = c.IdComprobante,
                    Folio       = c.Folio,
                    Factura = $"{c.Serie}-{c.Numero}",
                    FechaFactura = c.FechaEmision
                                        .ToString("yyyy-MM-dd"),
                    FechaDistribucion = c.FechaRecepcion.HasValue
                                        ? c.FechaRecepcion.Value
                                          .ToString("yyyy-MM-dd")
                                        : c.FechaEmision
                                          .ToString("yyyy-MM-dd"),
                    ImpoCompra = c.MontoNeto,
                    CargosVarios = c.MontoExento,
                    ImpVentas2 = c.MontoIGVCredito,
                    MntoFactura = c.MontoBruto,
                    ImpSinDesc = c.MontoIGVCredito,
                    FechaDcto = finMes.ToString("yyyy-MM-dd"),
                    DiasVto = int.TryParse(
                                        c.PlazoPago, out var dias)
                                        ? dias : 0,
                    FechaVen = c.FechaVencimiento.HasValue
                                        ? c.FechaVencimiento.Value
                                          .ToString("yyyy-MM-dd")
                                        : string.Empty,
                    Moneda = c.Moneda,
                    TipoCambio = c.TasaCambio,
                    CtaCP = primeraImp?.CuentaContable
                                        ?? string.Empty,
                    CtaCPUnid1 = primeraImp?.CodUnidad1Cuenta
                                        ?? string.Empty,
                    CtaCPUnid2 = string.Empty,
                    CtaCPUnid3 = primeraImp?.CodUnidad3Cuenta
                                        ?? string.Empty,
                    CtaCPUnid4 = primeraImp?.CodUnidad4Cuenta
                                        ?? string.Empty,
                    DescripcionCuenta = primeraImp?.DescripcionCuenta
                                        ?? string.Empty,
                    Ref = c.Observacion ?? string.Empty,
                    EstadoAut = "Autorizado",
                    Autorizo = c.RolAutorizacion ?? string.Empty,
                    Notas = c.Mensaje ?? string.Empty,
                    UsaDetraccion = c.TieneDetraccion ? "1" : "",
                    Detraccion = c.TipoDetraccion ?? string.Empty,
                    Tasa = c.PorcentajeDetraccion ?? 0,
                    TotalDetraccion = c.MontoDetraccion > 0
                        ? c.MontoDetraccion
                        : c.TieneDetraccion && c.PorcentajeDetraccion.HasValue
                            ? Math.Round(c.MontoTotal * c.PorcentajeDetraccion.Value / 100, 2)
                            : 0,
                    TotalDetLocal = c.MontoDetraccion > 0
                        ? (c.Moneda == "PEN" ? c.MontoDetraccion : Math.Round(c.MontoDetraccion * c.TasaCambio, 2))
                        : c.TieneDetraccion && c.PorcentajeDetraccion.HasValue
                            ? Math.Round(c.MontoTotal * c.PorcentajeDetraccion.Value / 100 * (c.Moneda == "PEN" ? 1 : c.TasaCambio), 2)
                            : 0,
                    MontoExento    = c.MontoExento,
                    MontoRetencion = c.MontoRetencion,
                    PorcentajeIGV  = c.PorcentajeIGV,
                    TipoSunat      = c.TipoSunat
                });
            }

            return result;
        }

        public async Task<IEnumerable<SytelineDistribucionDto>>
            ObtenerDistribucionSytelineAsync(List<string>? folios = null)
        {
            var query = _contexto.Comprobantes
                .Where(x => x.CodigoEstado == "APROBADO");

            if (folios != null && folios.Any())
                query = query.Where(x => folios.Contains(x.Folio));

            var comprobantes = await query
                .OrderBy(x => x.FechaAprobacion)
                .ToListAsync();

            // Lookup IdProveedorExternal para empleados (el RucReceptor es el proveedor pagado)
            var empleadoRucs = comprobantes
                .Where(c => c.EsEmpleado)
                .Select(c => c.RucReceptor)
                .Distinct()
                .ToList();

            var empleadoVendNums = empleadoRucs.Count > 0
                ? await _contexto.Proveedores
                    .Where(p => empleadoRucs.Contains(p.Ruc) && p.IdProveedorExternal != 0)
                    .ToDictionaryAsync(p => p.Ruc, p => p.IdProveedorExternal.ToString())
                : new Dictionary<string, string>();

            var resultado = new List<SytelineDistribucionDto>();

            foreach (var c in comprobantes)
            {
                var imputaciones = await _contexto.ImputacionesContables
                    .Where(x => x.Folio == c.Folio)
                    .OrderBy(x => x.Secuencia)
                    .ToListAsync();

                if (!imputaciones.Any()) continue;

                var tieneExento = c.MontoExento > 0;
                var cantDist    = tieneExento ? 3 : 2;

                var imputacionesDistribucion = imputaciones.Skip(1).Take(cantDist).ToList();
                if (imputacionesDistribucion.Count < 2) continue;

                var fechaDist = c.FechaRecepcion.HasValue
                    ? c.FechaRecepcion.Value.ToString("dd/MM/yyyy")
                    : c.FechaEmision.ToString("dd/MM/yyyy");

                for (int idx = 0; idx < imputacionesDistribucion.Count; idx++)
                {
                    var imp = imputacionesDistribucion[idx];
                    int secDist = (idx + 1) * 5;

                    string sistImpst, codImp, descCodImp;
                    decimal baseImp, importe;

                    switch (idx)
                    {
                        case 0:
                            sistImpst  = string.Empty;
                            codImp     = string.Empty;
                            descCodImp = string.Empty;
                            baseImp    = 0;
                            importe    = c.MontoNeto;
                            break;
                        case 1:
                            sistImpst  = "2";
                            codImp     = "IGV18";
                            descCodImp = "IGV 18%";
                            baseImp    = c.MontoNeto;
                            importe    = c.MontoIGVCredito;
                            break;
                        case 2:
                            sistImpst  = string.Empty;
                            codImp     = "EXO";
                            descCodImp = "Exento";
                            baseImp    = c.MontoExento;
                            importe    = c.MontoExento;
                            break;
                        default:
                            sistImpst  = string.Empty;
                            codImp     = string.Empty;
                            descCodImp = string.Empty;
                            baseImp    = 0;
                            importe    = imp.Monto;
                            break;
                    }

                    var proveedorExport = c.EsEmpleado ? (c.EmpleadoCodigo ?? string.Empty) : c.RucReceptor;
                    var nombreExport    = c.EsEmpleado ? (c.EmpleadoNombre ?? string.Empty) : c.RazonSocialReceptor;

                    var nroProvDist    = c.EsEmpleado && idx == 0 ? c.RucReceptor         : (idx == 0 ? (c.RucBeneficiario ?? string.Empty) : string.Empty);
                    var nomProvDist    = c.EsEmpleado && idx == 0 ? c.RazonSocialReceptor  : (idx == 0 ? (c.RazonSocialBenef ?? string.Empty) : string.Empty);

                    // Para empleados en línea principal:
                    //   TaxRegNum      = RUC del proveedor (ej. "2031665659")
                    //   aptZCO_APD     = IdProveedorExternal del proveedor (ej. "21")
                    // Para no-empleados en línea principal:
                    //   TaxRegNum      = RUC del beneficiario
                    //   aptZCO_APD     = vacío
                    string numRegFiscDist, aptZCO;
                    if (c.EsEmpleado && idx == 0)
                    {
                        empleadoVendNums.TryGetValue(c.RucReceptor, out var eVendId);
                        numRegFiscDist = c.RucReceptor;            // RUC del proveedor
                        aptZCO         = eVendId ?? string.Empty;  // IdProveedorExternal como string
                    }
                    else
                    {
                        numRegFiscDist = idx == 0 ? (c.RucBeneficiario ?? string.Empty) : string.Empty;
                        aptZCO         = string.Empty;
                    }

                    resultado.Add(new SytelineDistribucionDto
                    {
                        Proveedor         = proveedorExport,
                        Comprobante       = c.IdComprobante,
                        Nombre            = nombreExport,
                        FechaDistribucion = fechaDist,
                        Factura           = $"{c.Serie}-{c.Numero}",
                        FechaFactura      = c.FechaEmision.ToString("dd/MM/yyyy"),
                        TasaCambio        = c.TasaCambio,
                        Moneda            = c.Moneda,
                        ImpoCompra        = c.MontoNeto,
                        IGV               = c.MontoIGVCredito,
                        MntoFactura       = c.MontoTotal,
                        TotalDistribucion = c.MontoTotal,
                        NroProveedor      = nroProvDist,
                        NombreProv        = nomProvDist,
                        NumRegFiscal      = numRegFiscDist,
                        SecDist           = secDist,
                        Proyecto          = imp.Proyecto ?? string.Empty,
                        SistImpst         = sistImpst,
                        CodImp            = codImp,
                        DescCodImp        = descCodImp,
                        BaseImp           = baseImp,
                        Importe           = importe,
                        CuentaContable    = imp.CuentaContable ?? string.Empty,
                        DescripcionCuenta = imp.DescripcionCuenta ?? string.Empty,
                        CodUnidad1        = imp.CodUnidad1Cuenta ?? string.Empty,
                        CodUnidad3        = imp.CodUnidad3Cuenta ?? string.Empty,
                        CodUnidad4        = imp.CodUnidad4Cuenta ?? string.Empty,
                        EsLineaPrincipal     = idx == 0,
                        EsEmpleado           = c.EsEmpleado,
                        TipoDoc              = c.TipoSunat,
                        AptZCO_APD_VendNum   = aptZCO,
                        NombreProveedor      = c.RazonSocialReceptor
                    });
                }
            }

            return resultado;
        }
    }
}
