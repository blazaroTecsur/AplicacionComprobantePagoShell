using ComprobantePago.Application.DTOs.Responses;
using ComprobantePago.Application.Interfaces.QueryServices;
using ComprobantePago.Domain.Entities;
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

            // Índice RUC → IdProveedorExternal para todos (proveedores y empleados).
            // Empleados se identifican por EmpleadoCodigo (= Ruc en tmaproveedor, TipoPersona = "1").
            var rucs = comprobantes
                .Select(c => c.EsEmpleado ? (c.EmpleadoCodigo ?? string.Empty) : c.RucReceptor)
                .Where(r => !string.IsNullOrEmpty(r))
                .Distinct()
                .ToList();

            var vendNums = await _contexto.Proveedores
                .Where(p => rucs.Contains(p.Ruc) && !string.IsNullOrEmpty(p.IdProveedorExternal))
                .ToDictionaryAsync(p => p.Ruc, p => p.IdProveedorExternal!);

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
                var rucLookup = c.EsEmpleado ? (c.EmpleadoCodigo ?? string.Empty) : c.RucReceptor;
                var vendNum = vendNums.TryGetValue(rucLookup, out var vn) ? vn : string.Empty;

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
                    CtaCPUnid2 = primeraImp?.CodUnidad2Cuenta
                                        ?? string.Empty,
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
                    .Where(p => empleadoRucs.Contains(p.Ruc) && !string.IsNullOrEmpty(p.IdProveedorExternal))
                    .ToDictionaryAsync(p => p.Ruc, p => p.IdProveedorExternal!)
                : new Dictionary<string, string>();

            var resultado = new List<SytelineDistribucionDto>();

            foreach (var c in comprobantes)
            {
                var imputaciones = await _contexto.ImputacionesContables
                    .Where(x => x.Folio == c.Folio)
                    .OrderBy(x => x.Secuencia)
                    .ToListAsync();

                if (!imputaciones.Any()) continue;

                var imputacionesDistribucion = imputaciones.Skip(1).ToList();
                if (!imputacionesDistribucion.Any()) continue;

                // Líneas de distribución:
                // Si las imputaciones tienen TipoLinea (RP fraccionado), cada imputación
                // es una línea independiente con su propio monto (leído de rcoimputacioncontable).
                // Si no, se construyen desde los montos del comprobante (flujo normal).
                bool esFraccionado = imputacionesDistribucion.Any(i => i.TipoLinea != null);

                var distLines = new List<(string sistImpst, string codImp, string descCodImp, decimal baseImp, decimal importe, ImputacionContable imp)>();

                if (esFraccionado)
                {
                    var codIgv  = c.PorcentajeIGV == 10 ? "IGV10" : "IGV18";
                    var descIgv = c.PorcentajeIGV == 10 ? "IGV 10%" : "IGV 18%";
                    foreach (var imp in imputacionesDistribucion)
                    {
                        var (sist, cod, desc, baseImp) = imp.TipoLinea switch
                        {
                            "IGV"    => ("2", codIgv,  descIgv,  c.MontoNeto),
                            "EXENTO" => ("",  "EXO",   "Exento",  imp.Monto),
                            _        => ("",  "",      "",        0m)
                        };
                        distLines.Add((sist, cod, desc, baseImp, imp.Monto, imp));
                    }
                }
                else
                {
                    // Flujo normal: montos desde el comprobante, cuentas desde imputaciones por posición
                    var codIgv  = c.PorcentajeIGV == 10 ? "IGV10" : "IGV18";
                    var descIgv = c.PorcentajeIGV == 10 ? "IGV 10%" : "IGV 18%";
                    var montoLines = new List<(string sistImpst, string codImp, string descCodImp, decimal baseImp, decimal importe)>();
                    if (c.MontoNeto > 0)
                        montoLines.Add(("", "", "", 0, c.MontoNeto));
                    if (c.MontoIGVCredito > 0)
                        montoLines.Add(("2", codIgv, descIgv, c.MontoNeto, c.MontoIGVCredito));
                    if (c.MontoExento > 0)
                        montoLines.Add(("", "EXO", "Exento", c.MontoExento, c.MontoExento));

                    var totalLineas = Math.Min(montoLines.Count, imputacionesDistribucion.Count);
                    for (int i = 0; i < totalLineas; i++)
                        distLines.Add((montoLines[i].sistImpst, montoLines[i].codImp, montoLines[i].descCodImp,
                                       montoLines[i].baseImp,   montoLines[i].importe, imputacionesDistribucion[i]));
                }

                if (!distLines.Any()) continue;

                var fechaDist = c.FechaRecepcion.HasValue
                    ? c.FechaRecepcion.Value.ToString("dd/MM/yyyy")
                    : c.FechaEmision.ToString("dd/MM/yyyy");

                for (int idx = 0; idx < distLines.Count; idx++)
                {
                    var (sistImpst, codImp, descCodImp, baseImp, importe, imp) = distLines[idx];
                    int secDist = (idx + 1) * 5;

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
                        CodUnidad2        = imp.CodUnidad2Cuenta ?? string.Empty,
                        CodUnidad3        = imp.CodUnidad3Cuenta ?? string.Empty,
                        CodUnidad4        = imp.CodUnidad4Cuenta ?? string.Empty,
                        // Fraccionado: todas las líneas GRAVADO/EXENTO son líneas de gasto;
                        // solo IGV queda fuera. Normal: solo la primera línea GRAVADO (idx=0,
                        // codImp vacío) es línea de gasto; IGV y EXENTO se envían por separado.
                        EsLineaPrincipal     = esFraccionado ? !codImp.StartsWith("IGV")
                                                             : (idx == 0 && codImp != "EXO"),
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
