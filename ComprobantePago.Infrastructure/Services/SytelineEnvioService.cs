using ComprobantePago.Application.DTOs.Infor;
using ComprobantePago.Application.DTOs.Responses;
using ComprobantePago.Application.Interfaces.Services;
using ComprobantePago.Application.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace ComprobantePago.Infrastructure.Services
{
    /// <summary>
    /// Envía comprobantes aprobados al IDO SLAptrxs de Infor Syteline.
    /// </summary>
    public sealed class SytelineEnvioService : ISytelineEnvioService
    {
        private readonly ISytelineIdoService             _ido;
        private readonly InforSettings                   _settings;
        private readonly ILogger<SytelineEnvioService>  _logger;

        public SytelineEnvioService(
            ISytelineIdoService            ido,
            IOptions<InforSettings>        settings,
            ILogger<SytelineEnvioService>  logger)
        {
            _ido      = ido;
            _settings = settings.Value;
            _logger   = logger;
        }

        // ── Insertar una cabecera ─────────────────────────────────────────────

        public async Task<(int Voucher, string ItemId)> EnviarCabeceraAsync(
            SytelineCabeceraDto cabecera,
            CancellationToken ct = default)
        {
            // Validar que el proveedor tenga VendNum asignado
            var vendNum = cabecera.VendNum.PadLeft(7);
            if (string.IsNullOrWhiteSpace(vendNum.Trim()) || vendNum.Trim() == "0")
                throw new InvalidOperationException(
                    $"El proveedor del comprobante '{cabecera.Factura}' no tiene VendNum " +
                    $"asignado en el maestro de proveedores (IdProveedorExternal = 0 o vacío). " +
                    $"Sincronice el maestro de proveedores con Syteline antes de enviar.");

            // Obtener el siguiente número de voucher disponible en Syteline
            var voucher  = await ObtenerSiguienteVoucherAsync(ct);
            var dto      = MapearCabecera(cabecera, voucher);
            var propList = ConstruirPropiedades(dto);

            _logger.LogInformation(
                "Enviando comprobante {InvNum} de proveedor {VendNum} a SLAptrxs con Voucher {Voucher}...",
                dto.InvNum, dto.VendNum, voucher);

            var respuesta = await _ido.InsertItemAsync("SLAptrxs", propList,
                refresh: "PROPS", props: "Voucher", ct: ct);

            var voucherConfirmado = ExtraerVoucher(respuesta);
            var itemId            = ExtraerItemId(respuesta);
            var finalVoucher      = voucherConfirmado > 0 ? voucherConfirmado : voucher;

            _logger.LogInformation(
                "Comprobante {InvNum} insertado en SLAptrxs. Voucher: {Voucher} ItemId: {ItemId}",
                dto.InvNum, finalVoucher, itemId);

            return (finalVoucher, itemId);
        }

        // ── Insertar múltiples cabeceras ──────────────────────────────────────

        public async Task<IEnumerable<int>> EnviarCabecerasAsync(
            IEnumerable<SytelineCabeceraDto> cabeceras,
            CancellationToken ct = default)
        {
            var vouchers = new List<int>();

            foreach (var cabecera in cabeceras)
            {
                ct.ThrowIfCancellationRequested();
                var (voucher, _) = await EnviarCabeceraAsync(cabecera, ct);
                vouchers.Add(voucher);
            }

            return vouchers;
        }

        // ── Eliminar cabecera (rollback compensatorio) ────────────────────────

        public async Task EliminarCabeceraAsync(string itemId, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                _logger.LogWarning("No se puede revertir cabecera: ItemId vacío.");
                return;
            }

            try
            {
                await _ido.DeleteItemAsync("SLAptrxs", itemId, ct);
                _logger.LogInformation("Cabecera Syteline revertida. ItemId={ItemId}", itemId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "No se pudo revertir cabecera Syteline. ItemId={ItemId} — requiere limpieza manual.",
                    itemId);
                throw;
            }
        }

        // ── Obtener siguiente número de Voucher ───────────────────────────────
        // Consulta el máximo Voucher existente en SLAptrxs para este site y
        // devuelve max + 1. Si no hay registros, devuelve 1.

        public async Task<int> ObtenerSiguienteVoucherAsync(CancellationToken ct = default)
        {
            try
            {
                var resultado = await _ido.LoadAsync(
                    "SLAptrxs",
                    props:     "Voucher",
                    orderBy:   "Voucher DESC",
                    recordCap: 1,
                    adv:       true,
                    ct:        ct);

                _logger.LogInformation("SLAptrxs MaxVoucher: {Body}", resultado.GetRawText());

                var max = ExtraerVoucherDeItems(resultado);
                var siguiente = max + 1;

                _logger.LogInformation("Próximo Voucher a usar: {Voucher}", siguiente);
                return siguiente;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo obtener el último Voucher de SLAptrxs. Se usará 1.");
                return 1;
            }
        }

        // ── Insertar distribución (SLAptrxds) ─────────────────────────────────

        public async Task EnviarDistribucionAsync(
            SytelineCabeceraDto cabecera,
            IEnumerable<SytelineDistribucionDto> lineas,
            int voucher,
            CancellationToken ct = default)
        {
            var lista   = lineas.OrderBy(l => l.SecDist).ToList();
            var vendNum = cabecera.VendNum.PadLeft(7);
            var distSeq = 5;

            if (lista.Count == 0)
            {
                _logger.LogWarning("Sin líneas de distribución para {Factura}", cabecera.Factura);
                return;
            }

            var lineaIgv    = lista.FirstOrDefault(l => l.CodImp == "IGV18");
            var lineaExento = lista.FirstOrDefault(l => l.CodImp == "EXO");
            var expenseLines = lista.Where(l => l.EsLineaPrincipal).ToList();

            // 1. Líneas de gasto (una por cada imputación principal)
            foreach (var linea in expenseLines)
            {
                var dto = new SLAptrxdsInsertDto
                {
                    VendNum     = vendNum,
                    Voucher     = voucher,
                    DistSeq     = distSeq,
                    Acct        = linea.CuentaContable[..Math.Min(12, linea.CuentaContable.Length)],
                    AcctUnit1   = linea.CodUnidad1.Length > 0 ? linea.CodUnidad1[..Math.Min(4, linea.CodUnidad1.Length)] : "",
                    AcctUnit3   = linea.CodUnidad3.Length > 0 ? linea.CodUnidad3[..Math.Min(4, linea.CodUnidad3.Length)] : "",
                    AcctUnit4   = linea.CodUnidad4.Length > 0 ? linea.CodUnidad4[..Math.Min(4, linea.CodUnidad4.Length)] : "",
                    Amount      = linea.Importe,
                    DIOTTransType      = linea.EsEmpleado ? "1" : "0",
                    TaxRegNum          = linea.EsEmpleado && linea.NumRegFiscal.Length > 0 ? linea.NumRegFiscal[..Math.Min(25, linea.NumRegFiscal.Length)] : "",
                    TaxRegNumType      = linea.EsEmpleado && !string.IsNullOrEmpty(linea.NumRegFiscal) ? "T" : "",
                    ProjNum            = linea.Proyecto.Length > 0 ? linea.Proyecto[..Math.Min(10, linea.Proyecto.Length)] : "",
                    aptZCO_APD_VendNum  = linea.EsEmpleado && linea.AptZCO_APD_VendNum.Length > 0 ? linea.AptZCO_APD_VendNum : "",
                    aptZLA_TipoDocumento = linea.TipoDoc.Length > 0 ? linea.TipoDoc[..Math.Min(2, linea.TipoDoc.Length)] : "",
                    VendorName           = linea.EsEmpleado && linea.NombreProveedor.Length > 0 ? linea.NombreProveedor[..Math.Min(60, linea.NombreProveedor.Length)] : "",
                    ForeignTaxRegNum     = linea.EsEmpleado ? "0" : "",
                };
                _logger.LogInformation("IDO SLAptrxds Gasto → Voucher={Voucher} DistSeq={Seq} Acct={Acct} Amount={Amt}",
                    voucher, distSeq, dto.Acct, dto.Amount);
                await _ido.InsertItemAsync("SLAptrxds", ConstruirPropiedades(dto), ct: ct);
                distSeq += 5;
            }

            // 2. Línea IGV
            if (lineaIgv != null)
            {
                var dto = new SLAptrxdsInsertDto
                {
                    VendNum   = vendNum,
                    Voucher   = voucher,
                    DistSeq   = distSeq,
                    Acct      = lineaIgv.CuentaContable[..Math.Min(12, lineaIgv.CuentaContable.Length)],
                    AcctUnit1 = lineaIgv.CodUnidad1.Length > 0 ? lineaIgv.CodUnidad1[..Math.Min(4, lineaIgv.CodUnidad1.Length)] : "",
                    AcctUnit3 = lineaIgv.CodUnidad3.Length > 0 ? lineaIgv.CodUnidad3[..Math.Min(4, lineaIgv.CodUnidad3.Length)] : "",
                    AcctUnit4 = lineaIgv.CodUnidad4.Length > 0 ? lineaIgv.CodUnidad4[..Math.Min(4, lineaIgv.CodUnidad4.Length)] : "",
                    Amount    = lineaIgv.Importe,
                    TaxBasis  = lineaIgv.BaseImp,
                    TaxCode   = "IGV18",
                    TaxSystem = "2",
                };
                _logger.LogInformation("IDO SLAptrxds IGV → Voucher={Voucher} DistSeq={Seq} Amount={Amt}",
                    voucher, distSeq, dto.Amount);
                await _ido.InsertItemAsync("SLAptrxds", ConstruirPropiedades(dto), ct: ct);
                distSeq += 5;
            }

            // 4. Línea exento
            if (lineaExento != null)
            {
                var dto = new SLAptrxdsInsertDto
                {
                    VendNum  = vendNum,
                    Voucher  = voucher,
                    DistSeq  = distSeq,
                    Acct     = lineaExento.CuentaContable[..Math.Min(12, lineaExento.CuentaContable.Length)],
                    AcctUnit1 = lineaExento.CodUnidad1.Length > 0 ? lineaExento.CodUnidad1[..Math.Min(4, lineaExento.CodUnidad1.Length)] : "",
                    AcctUnit3 = lineaExento.CodUnidad3.Length > 0 ? lineaExento.CodUnidad3[..Math.Min(4, lineaExento.CodUnidad3.Length)] : "",
                    AcctUnit4 = lineaExento.CodUnidad4.Length > 0 ? lineaExento.CodUnidad4[..Math.Min(4, lineaExento.CodUnidad4.Length)] : "",
                    Amount    = lineaExento.Importe,
                    TaxCodeE  = "EXE",
                };
                _logger.LogInformation("IDO SLAptrxds Exento → Voucher={Voucher} DistSeq={Seq} Amount={Amt}",
                    voucher, distSeq, dto.Amount);
                await _ido.InsertItemAsync("SLAptrxds", ConstruirPropiedades(dto), ct: ct);
            }
        }

        // ── Mapeo SytelineCabeceraDto → SLAptrxsInsertDto ────────────────────

        private SLAptrxsInsertDto MapearCabecera(SytelineCabeceraDto c, int voucher) => new()
        {
            // "V" para facturas y demás; vacío para NC/ND (07/08) — se omite al filtrar
            Type = c.TipoSunat is "07" or "08" ? "" : "V",

            // VendNum: longitud fija de 7 caracteres, rellenado con espacios a la izquierda
            VendNum  = c.VendNum.PadLeft(7),
            Voucher  = voucher,
            InvDate  = c.FechaFactura,
            DistDate = c.FechaDistribucion,
            UbToSite = _settings.Site,

            // Cabecera
            InvNum      = c.Factura[..Math.Min(22, c.Factura.Length)],
            PurchAmt    = c.ImpoCompra,
            SalesTax_2  = c.ImpVentas2,
            InvAmt      = c.MntoFactura,
            MiscCharges = c.CargosVarios,
            NonDiscAmt  = c.CargosVarios,

            // Vencimiento
            DueDate  = c.FechaVen,
            DueDays  = c.DiasVto,
            DiscDate = c.FechaDcto,
            DiscPct  = 0.000m,

            // Moneda
            AptCurrCode = c.Moneda,
            ExchRate    = c.TipoCambio,

            // Cuenta A/P
            ApAcct      = c.CtaCP[..Math.Min(12, c.CtaCP.Length)],
            ApAcctUnit1 = c.CtaCPUnid1[..Math.Min(4, c.CtaCPUnid1.Length)],
            ApAcctUnit3 = c.CtaCPUnid3[..Math.Min(4, c.CtaCPUnid3.Length)],
            ApAcctUnit4 = c.CtaCPUnid4[..Math.Min(4, c.CtaCPUnid4.Length)],

            // Referencia
            Ref        = c.Ref[..Math.Min(30, c.Ref.Length)],
            Txt        = c.Notas[..Math.Min(40, c.Notas.Length)],
            Authorizer = c.Autorizo[..Math.Min(128, c.Autorizo.Length)],

            // Impuesto: EXE cuando no hay IGV o hay monto exento; NR en caso contrario
            TaxCode1   = (c.ImpVentas2 == 0 || c.MontoExento > 0) ? "EXE" : "NR",
            AuthStatus = "F",

            // Folio de origen
            aptZLA_SeqFac = c.Comprobante.ToString(),

            // Detracción
            aptZLA_UsaDetraccion        = c.UsaDetraccion == "1" ? (byte)1 : (byte)0,
            aptZLA_CodigoDetraccion     = c.Detraccion,
            aptZLA_TasaDetraccion       = c.Tasa,
            aptZLA_TotalDetraccion      = c.TotalDetraccion,
            aptZLA_TotalDetraccionLocal = c.TotalDetLocal,
        };

        // ── Construir lista de IdoProperty ───────────────────────────────────
        // Todos los campos se envían con IsNull=false.
        // Se omiten únicamente strings vacíos y nulls.
        private static List<IdoProperty> ConstruirPropiedades(object dto)
        {
            var lista = new List<IdoProperty>();

            foreach (var prop in dto.GetType().GetProperties())
            {
                var valor = prop.GetValue(dto);

                if (valor is string s && string.IsNullOrEmpty(s)) continue;
                if (valor is null) continue;

                lista.Add(new IdoProperty
                {
                    IsNull = false,
                    Name   = prop.Name,
                    Value  = FormatearValor(valor)
                });
            }

            return lista;
        }

        private static string FormatearValor(object? valor) => valor switch
        {
            null       => "",
            decimal d  => d.ToString(System.Globalization.CultureInfo.InvariantCulture),
            double  db => db.ToString(System.Globalization.CultureInfo.InvariantCulture),
            float   f  => f.ToString(System.Globalization.CultureInfo.InvariantCulture),
            _          => valor.ToString() ?? ""
        };

        // ── Extraer Voucher del response de additem ───────────────────────────
        // /additem/adv devuelve UpdatedItems[0].Properties[] con Name/Value.

        private static int ExtraerVoucher(JsonElement respuesta)
        {
            if (respuesta.TryGetProperty("UpdatedItems", out var items) &&
                items.GetArrayLength() > 0)
            {
                var item = items[0];
                if (item.TryGetProperty("Properties", out var props) &&
                    props.ValueKind == JsonValueKind.Array)
                {
                    foreach (var prop in props.EnumerateArray())
                    {
                        if (prop.TryGetProperty("Name",  out var name)  &&
                            name.GetString() == "Voucher"                &&
                            prop.TryGetProperty("Value", out var value)  &&
                            int.TryParse(value.GetString(), out var v))
                        {
                            return v;
                        }
                    }
                }
            }

            return 0;
        }

        // ── Extraer ItemId del response de additem ────────────────────────────

        private static string ExtraerItemId(JsonElement respuesta)
        {
            if (respuesta.TryGetProperty("UpdatedItems", out var items) &&
                items.GetArrayLength() > 0 &&
                items[0].TryGetProperty("ItemID", out var itemId))
            {
                return itemId.GetString() ?? string.Empty;
            }
            return string.Empty;
        }

        // ── Extraer Voucher del response de LoadCollection (/adv) ─────────────
        // Items[n] es un array de {Name, Value} — tomamos el primero (orderBy DESC).

        private static int ExtraerVoucherDeItems(JsonElement resultado)
        {
            if (!resultado.TryGetProperty("Items", out var items) || items.GetArrayLength() == 0)
                return 0;

            var firstItem = items[0];

            // Formato /adv: Items[0] es array de {Name, Value}
            if (firstItem.ValueKind == JsonValueKind.Array)
            {
                foreach (var prop in firstItem.EnumerateArray())
                {
                    if (prop.TryGetProperty("Name",  out var name) &&
                        name.GetString() == "Voucher"              &&
                        prop.TryGetProperty("Value", out var value) &&
                        int.TryParse(value.GetString(), out var v))
                    {
                        return v;
                    }
                }
            }
            // Formato sin /adv: Items[0] es objeto con claves directas
            else if (firstItem.TryGetProperty("Voucher", out var vp))
            {
                if (vp.ValueKind == JsonValueKind.Number && vp.TryGetInt32(out var vi)) return vi;
                if (vp.ValueKind == JsonValueKind.String && int.TryParse(vp.GetString(), out var vs)) return vs;
            }

            return 0;
        }
    }
}
