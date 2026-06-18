using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Reciclaje.Aplicacion.Configuracion;
using Reciclaje.Aplicacion.Interfaces;
using Reciclaje.Dominio.Entidades;
using Reciclaje.Dominio.Interfaces;

namespace Reciclaje.Aplicacion.Servicios;

public class SyteLineServicio : ISyteLineServicio
{
    private readonly ISyteLineRepositorio _repositorio;
    private readonly ISyteLineHttpClientFactory _httpFactory;
    private readonly SyteLineConfig _cfg;

    public SyteLineServicio(
        ISyteLineRepositorio repositorio,
        ISyteLineHttpClientFactory httpFactory,
        IOptions<SyteLineConfig> options)
    {
        _repositorio = repositorio;
        _httpFactory = httpFactory;
        _cfg = options.Value;
    }

    // ─── Helpers privados ────────────────────────────────────────────────────

    private static readonly Regex _regexMs = new(@"/Date\((\d+)", RegexOptions.Compiled);
    private static readonly Regex _regexIdo = new(@"^(\d{4})(\d{2})(\d{2})\s(\d{2}):(\d{2}):(\d{2})", RegexOptions.Compiled);

    private static DateTime? ParseTransDate(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;

        var ms = _regexMs.Match(raw);
        var ido = _regexIdo.Match(raw);

        if (ms.Success)
            return DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(ms.Groups[1].Value)).UtcDateTime;

        if (ido.Success)
            return new DateTime(
                int.Parse(ido.Groups[1].Value), int.Parse(ido.Groups[2].Value),
                int.Parse(ido.Groups[3].Value), int.Parse(ido.Groups[4].Value),
                int.Parse(ido.Groups[5].Value), int.Parse(ido.Groups[6].Value));

        return DateTime.TryParse(raw, out var d) ? d : null;
    }

    private static string NowSyteLine()
    {
        var d = DateTime.Now;
        return $"{d:yyyyMMdd} {d:HH:mm:ss}.{d.Millisecond:D3}";
    }

    private static string? Str(JsonElement el, string key) =>
        el.TryGetProperty(key, out var v) && v.ValueKind != JsonValueKind.Null
            ? v.GetString()
            : null;

    // ─── 1. Token OAuth2 ─────────────────────────────────────────────────────

    public async Task<JsonElement> GetTokenAsync(
        string accessTokenUrl, string clientId, string clientSecret,
        string username, string password)
    {
        var basic = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

        var json = await _httpFactory.PostFormAsync(
            url: accessTokenUrl,
            basicAuth: basic,
            form: new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["username"] = username,
                ["password"] = password
            });

        return JsonSerializer.Deserialize<JsonElement>(json);
    }

    // ─── 2. GET IDO ──────────────────────────────────────────────────────────

    public async Task<JsonElement> IdoGetAsync(
        string endpointUrl, string accessToken, string mongooseConfig)
    {
        var json = await _httpFactory.GetAsync(endpointUrl, accessToken, mongooseConfig);
        return JsonSerializer.Deserialize<JsonElement>(json);
    }

    // ─── 3. PUT IDO ──────────────────────────────────────────────────────────

    public async Task<JsonElement> IdoPutAsync(
        string endpointUrl, string accessToken, string mongooseConfig, object payload)
    {
        var jsonBody = JsonSerializer.Serialize(payload);
        var json = await _httpFactory.PutAsync(endpointUrl, accessToken, mongooseConfig, jsonBody);
        return JsonSerializer.Deserialize<JsonElement>(json);
    }

    // ─── 4. INSERT integracionsyteline ───────────────────────────────────────

    public async Task<(int insertados, int omitidos, List<string> errores)>
        InsertIntegracionAsync(IEnumerable<JsonElement> items)
    {
        int ins = 0, skip = 0;
        var errores = new List<string>();
        var now = DateTime.Now;

        foreach (var item in items)
        {
            try
            {
                var rowPointer = Str(item, "RowPointer");
                if (rowPointer is null) { skip++; continue; }

                if (await _repositorio.ExisteIntegracion(rowPointer)) { skip++; continue; }

                await _repositorio.InsertarIntegracion(new Integracionsyteline
                {
                    RowPointer = rowPointer,
                    Sitio = Str(item, "SiteSite"),
                    Sro = Str(item, "SroNum"),
                    SroLine = item.TryGetProperty("SroLine", out var sl) ? int.Parse(sl.GetString()!) : null,
                    SroOper = item.TryGetProperty("SroOper", out var so) ? int.Parse(so.GetString()!) : null,
                    TransNum = item.TryGetProperty("TransNum", out var tn) ? int.Parse(tn.GetString()!) : null,
                    TransDate = ParseTransDate(Str(item, "TransDate")),
                    Articulo = Str(item, "Item"),
                    Estado = "Pendiente",
                    Posted = 0,
                    FechaCreacion = now,
                    FechaModificacion = now
                });

                ins++;
            }
            catch (Exception ex) { errores.Add(ex.Message); }
        }

        return (ins, skip, errores);
    }

    // ─── 5. INSERT sro + srolinea ────────────────────────────────────────────

    public async Task<object> InsertSroAsync(IEnumerable<JsonElement> items)
    {
        int sroIns = 0, sroSkip = 0, linIns = 0, linSkip = 0, convOk = 0, convMiss = 0;
        var errores = new List<object>();
        var now = DateTime.Now;

        foreach (var item in items)
        {
            try
            {
                var sroNum = Str(item, "SroNum");
                var rowPointer = Str(item, "RowPointer");

                if (sroNum is null) { sroSkip++; continue; }

                // 5a. INSERT IGNORE cabecera sro
                try
                {
                    await _repositorio.InsertarSro(new Sro
                    {
                        NumeroSro = sroNum,
                        Sitio = Str(item, "SiteSite"),
                        FechaCreacionAudit = now
                    });
                    sroIns++;
                }
                catch { sroSkip++; }

                // 5b. Conversión artículo
                var itemCod = item.TryGetProperty("Item", out var itProp)
                    ? itProp.GetString()?.TrimStart('0')
                    : null;

                string? artReciclaje = null;
                int? conversionId = null;

                if (!string.IsNullOrEmpty(itemCod))
                {
                    var resultado = await _repositorio.ObtenerArticuloReciclaje(itemCod);
                    artReciclaje = resultado.ArticuloReciclaje;
                    conversionId = resultado.ConversionId;
                    if (artReciclaje is not null) convOk++; else convMiss++;
                }
                else convMiss++;

                // 5c. INSERT srolinea
                try
                {
                    var fechaTrans = ParseTransDate(Str(item, "TransDate"));
                    var hoy = DateTime.Now;
                    string? nombrePo = await _repositorio.ObtenerNombrePoAsync((short)hoy.Year, (byte)hoy.Month);

                    await _repositorio.InsertarSrolinea(new Srolinea
                    {
                        Sroid = await _repositorio.ObtenerSroidPorNumero(sroNum),
                        SroLineaSL = item.TryGetProperty("SroLine", out var slN) ? int.Parse(slN.GetString()!) : 0,
                        RowPointer = rowPointer,
                        CodigoAlmacenNoInv = Str(item, "Whse"),
                        ArticuloNoInv = Str(item, "Item") ?? string.Empty,
                        UmnoInv = Str(item, "UM"),
                        CantidadNoInv = item.TryGetProperty("MatlQty", out var mq) && mq.ValueKind != JsonValueKind.Null
                                                ? (decimal?)Convert.ToDecimal(mq.GetString()) : null,
                        FechaTransaccion = fechaTrans,
                        ArticuloReciclaje = artReciclaje,
                        UMReciclaje = "KG",
                        OrdenCompra = nombrePo,
                        ConversionID = conversionId,
                        TramaSyteLine = item.GetRawText(),
                        Dept = Str(item, "Dept"),
                        EstadoLinea = "Creación",
                        FechaCreacionAudit = now
                    });
                    linIns++;
                }
                catch { linSkip++; }
            }
            catch (Exception ex)
            {
                errores.Add(new { rowPointer = Str(item, "RowPointer") ?? "?", error = ex.Message });
            }
        }

        return new
        {
            sroInserted = sroIns,
            sroSkipped = sroSkip,
            lineaInserted = linIns,
            lineaSkipped = linSkip,
            conversionFound = convOk,
            conversionMissing = convMiss,
            errores
        };
    }

    // ─── 6. Build payload para PUT IDO ───────────────────────────────────────

    private static readonly HashSet<string> _camposEliminar =
    [
        "DocumentNum", "DropCustNum", "DropShipNo", "DropUsrNum",
        "DropCustSeq", "DropUsrSeq", "RowPointer", "_ItemId"
    ];

    public async Task<object> BuildPayloadAsync(IEnumerable<int>? sroLineaIds = null)
    {
        var lineas = await _repositorio.ObtenerLineasConVales(sroLineaIds);
        var payloads = new List<object>();
        var errores = new List<object>();

        foreach (var linea in lineas)
        {
            foreach (var vale in linea.Valerecuperos.OrderBy(v => v.ValeId))
            {
                try
                {
                    var trama = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(linea.TramaSyteLine!)!;
                    var qty = vale.CantidadRecibida is not null ? (double)vale.CantidadRecibida : (double?)null;

                    if (qty.HasValue)
                    {
                        trama["MatlQty"] = JsonDocument.Parse(qty.Value.ToString("G")).RootElement;
                        trama["MatlQtyConv"] = JsonDocument.Parse(qty.Value.ToString("G")).RootElement;
                    }
                    trama["PostDate"] = JsonDocument.Parse($"\"{NowSyteLine()}\"").RootElement;
                    trama.Remove("PostedDate");
                    trama["Type"] = JsonDocument.Parse("\"A\"").RootElement;
                    foreach (var k in _camposEliminar) trama.Remove(k);

                    var properties = trama.Select(kv => new
                    {
                        IsNull = kv.Value.ValueKind == JsonValueKind.Null,
                        Modified = true,
                        Name = kv.Key,
                        Value = kv.Value.ValueKind == JsonValueKind.Null ? null : (object?)kv.Value.ToString()
                    }).ToList();

                    payloads.Add(new
                    {
                        _meta = new
                        {
                            linea.SrolineaId,
                            linea.RowPointer,
                            vale.ValeId,
                            vale.NumeroVale,
                            vale.CantidadRecibida
                        },
                        payload = new { Action = 1, ItemId = "PBT=[fs_sro_matl]", Properties = properties }
                    });
                }
                catch (Exception ex)
                {
                    errores.Add(new { linea.SrolineaId, vale.NumeroVale, error = ex.Message });
                }
            }
        }

        return new { success = true, total = payloads.Count, errores, payloads };
    }

    // ─── 7. GetCostoUnitario ──────────────────────────────────────────────────

    public async Task<decimal?> GetCostoUnitarioAsync(
        string item, string accessToken, string mongooseConfig)
    {
        try
        {
            var encodedItem = Uri.EscapeDataString(item);
            var url = $"{_cfg.NonInventoryItemsUrl}?filter=Item='{encodedItem}'";

            var json = await _httpFactory.GetAsync(url, accessToken, mongooseConfig);
            var doc = JsonSerializer.Deserialize<JsonElement>(json);

            if (!doc.TryGetProperty("Items", out var items) ||
                items.ValueKind != JsonValueKind.Array)
                return null;

            foreach (var row in items.EnumerateArray())
            {
                if (row.ValueKind != JsonValueKind.Array) continue;

                foreach (var prop in row.EnumerateArray())
                {
                    if (prop.TryGetProperty("Name", out var nameProp) &&
                        nameProp.GetString() == "UnitCost" &&
                        prop.TryGetProperty("Value", out var valueProp) &&
                        valueProp.ValueKind != JsonValueKind.Null)
                    {
                        var raw = valueProp.GetString();
                        if (decimal.TryParse(raw,
                            System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture,
                            out var costo))
                            return costo;
                    }
                }
            }
        }
        catch
        {
            // Si falla la consulta de costo no bloqueamos el flujo principal
        }
        return null;
    }
}
