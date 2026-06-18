using Microsoft.Extensions.Options;
using Reciclaje.Aplicacion.Configuracion;
using Reciclaje.Aplicacion.Interfaces;
using Reciclaje.Dominio.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Reciclaje.Aplicacion.Servicios;

public class SyteLinePoServicio : ISyteLinePoServicio
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly ITareaOrdenCompraRepositorio _tareaRepositorio;
    private readonly SyteLineConfig _cfg;

    public SyteLinePoServicio(
        IHttpClientFactory httpFactory,
        ITareaOrdenCompraRepositorio tareaRepositorio,
        IOptions<SyteLineConfig> options)
    {
        _httpFactory = httpFactory;
        _tareaRepositorio = tareaRepositorio;
        _cfg = options.Value;
    }

    // ── CrearOrdenCompraAsync ────────────────────────────────────────────────
    public async Task<(bool exitoso, string mensaje)> CrearOrdenCompraAsync(
        int tareaId,
        short anno,
        byte mes,
        string nombrePo,
        string? sitio,
        string accessToken,
        string mongooseConfig)
    {
        var orderDate = new DateTime(anno, mes, 1);
        var orderDateStr = $"{orderDate:yyyyMMdd} 00:00:00.000";
        var sitioValor = sitio ?? _cfg.SitioDefault;

        var payload = new
        {
            Action = 1,
            ItemId = "PBT=[po]",
            Properties = new[]
            {
                Prop("OrderDate",     orderDateStr),
                Prop("ParSite",       sitioValor),
                Prop("PoCurrCode",    _cfg.CurrCode),
                Prop("SitSiteName",   sitioValor),
                Prop("Stat",          "P"),
                Prop("TermsCode",     _cfg.TermsCode),
                Prop("TermsCodeDesc", _cfg.TermsCodeDesc),
                Prop("Type",          "R"),
                Prop("VenCurrCode",   _cfg.CurrCode),
                Prop("VendNum",       _cfg.VendNum),
                Prop("VendOrder",     _cfg.VendOrder),
                Prop("VendorName",    _cfg.VendorName),
                Prop("Whse",          _cfg.Whse),
                Prop("PoNum",         nombrePo)
            }
        };

        var http = BuildHttpClient(accessToken, mongooseConfig);
        var content = ToJsonContent(payload);

        HttpResponseMessage response;
        try
        {
            response = await http.PostAsync(_cfg.PoAddItemUrl, content);
        }
        catch (Exception ex)
        {
            await EliminarTareaAsync(tareaId);
            return (false, $"Error de conexión con SyteLine: {ex.Message}");
        }

        var responseBody = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            try
            {
                var json = JsonSerializer.Deserialize<JsonElement>(responseBody);
                if (json.TryGetProperty("ErrorMessage", out var errProp) &&
                    !string.IsNullOrWhiteSpace(errProp.GetString()))
                {
                    await EliminarTareaAsync(tareaId);
                    return (false, $"SyteLine respondió con error: {errProp.GetString()!}");
                }
            }
            catch { /* si no es JSON válido se asume OK */ }

            return (true, $"Orden de Compra '{nombrePo}' creada correctamente en SyteLine.");
        }

        await EliminarTareaAsync(tareaId);
        return (false, $"SyteLine devolvió HTTP {(int)response.StatusCode}: {responseBody}");
    }

    // ── AgregarLineaPoAsync ──────────────────────────────────────────────────
    public async Task<(bool exitoso, string mensaje, int nuevaPoLine)> AgregarLineaPoAsync(
        string poNum,
        int ultimaLinea,
        string codigoArticulo,
        string descripcionArticulo,
        decimal cantidad,
        string unidadMedida,
        short anno,
        byte mes,
        string accessToken,
        string mongooseConfig,
        string articuloNoInv,
        decimal costoUnitario)
    {
        var nuevaPoLine = ultimaLinea + 1;
        var ultimoDiaMes = new DateTime(anno, mes, DateTime.DaysInMonth(anno, mes));
        var dueDateStr = $"{ultimoDiaMes:yyyyMMdd} 00:00:00.000";
        var recordDateStr = $"{DateTime.Now:yyyyMMdd HH:mm:ss.fff}";

        var payload = new
        {
            Action = 1,
            ItemId = "PBT=[poitem]",
            Properties = new[]
            {
                Prop("CurrCode",        _cfg.CurrCode),
                Prop("CurrCodeDesc",    _cfg.CurrCodeDesc),
                Prop("DueDate",         dueDateStr),
                Prop("Item",            articuloNoInv),
                Prop("ItmDescription",  descripcionArticulo),
                Prop("UnitMatCostConv", $"{costoUnitario:F5}"),
                Prop("PoLine",          nuevaPoLine.ToString()),
                Prop("PoNum",           poNum),
                Prop("PoStat",          "P"),
                Prop("PoVendorPo",      _cfg.VendOrder),
                Prop("QtyOrdered",      $"{cantidad:F8}"),
                Prop("QtyOrderedConv",  $"{cantidad:F8}"),
                Prop("RecordDate",      recordDateStr),
                Prop("TaxCode2",        _cfg.TaxCode2),
                Prop("TaxCode2Desc",    _cfg.TaxCode2Desc),
                Prop("UM",              unidadMedida.ToUpper()),
                Prop("Whse",            _cfg.Whse),
                Prop("NonInvAcct",      _cfg.NonInvAcct)
            }
        };

        var http = BuildHttpClient(accessToken, mongooseConfig);
        var content = ToJsonContent(payload);

        HttpResponseMessage response;
        try
        {
            response = await http.PostAsync(_cfg.PoItemAddItemUrl, content);
        }
        catch (Exception ex)
        {
            return (false, $"Error de conexión al agregar línea PO: {ex.Message}", nuevaPoLine);
        }

        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return (false, $"SyteLine HTTP {(int)response.StatusCode}: {body}", nuevaPoLine);

        try
        {
            var json = JsonSerializer.Deserialize<JsonElement>(body);

            if (json.TryGetProperty("MessageCode", out var code) && code.GetInt32() == 200)
                return (true, "Línea PO insertada correctamente.", nuevaPoLine);

            if (json.TryGetProperty("Message", out var msg))
                return (false, $"SyteLine: {msg.GetString()}", nuevaPoLine);
        }
        catch { /* respuesta no JSON — si HTTP fue 2xx se considera éxito */ }

        return (response.IsSuccessStatusCode, body, nuevaPoLine);
    }

    // ── ObtenerUltimaLineaPoAsync ────────────────────────────────────────────
    public async Task<(bool exitoso, string mensaje, int? ultimaLinea, string? rowPointer)>
        ObtenerUltimaLineaPoAsync(string poNum, string accessToken, string mongooseConfig)
    {
        var url = $"{_cfg.PoItemsUrl}?filter=PoNum='{poNum}'";
        var http = BuildHttpClient(accessToken, mongooseConfig);

        HttpResponseMessage response;
        try
        {
            response = await http.GetAsync(url);
        }
        catch (Exception ex)
        {
            return (false, $"Error de conexión al consultar SLPoItems: {ex.Message}", null, null);
        }

        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return (false, $"SyteLine devolvió HTTP {(int)response.StatusCode}: {body}", null, null);

        try
        {
            var json = JsonSerializer.Deserialize<JsonElement>(body);

            if (json.TryGetProperty("MessageCode", out var code) && code.GetInt32() != 0)
            {
                var msg = json.TryGetProperty("Message", out var m) ? m.GetString() : "Error desconocido";
                return (false, $"SyteLine error: {msg}", null, null);
            }

            if (!json.TryGetProperty("Items", out var items) || items.ValueKind != JsonValueKind.Array)
                return (false, "Respuesta sin ítems de SLPoItems.", null, null);

            int? maxPoLine = null;
            string? lastRowPtr = null;

            foreach (var item in items.EnumerateArray())
            {
                int? poLine = null;
                string? rp = null;

                foreach (var prop in item.EnumerateArray())
                {
                    if (!prop.TryGetProperty("Name", out var nameEl) ||
                        !prop.TryGetProperty("Value", out var valueEl)) continue;

                    var name = nameEl.GetString();
                    var value = valueEl.GetString();

                    if (name == "PoLine" && int.TryParse(value, out var pl)) poLine = pl;
                    if (name == "RowPointer") rp = value;
                }

                if (poLine.HasValue && (maxPoLine is null || poLine > maxPoLine))
                {
                    maxPoLine = poLine;
                    lastRowPtr = rp;
                }
            }

            if (maxPoLine is null)
                return (false, "No se encontraron líneas para la Orden de Compra.", null, null);

            return (true, "OK", maxPoLine, lastRowPtr);
        }
        catch (Exception ex)
        {
            return (false, $"Error al parsear respuesta de SLPoItems: {ex.Message}", null, null);
        }
    }

    // ── Helpers privados ─────────────────────────────────────────────────────

    private HttpClient BuildHttpClient(string accessToken, string mongooseConfig)
    {
        var http = _httpFactory.CreateClient();
        http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);
        http.DefaultRequestHeaders.Add("X-Infor-MongooseConfig", mongooseConfig);
        return http;
    }

    private static StringContent ToJsonContent(object payload) =>
        new(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

    private static object Prop(string name, string value) => new
    {
        IsNull = false,
        Modified = true,
        Name = name,
        Value = value
    };

    private async Task EliminarTareaAsync(int tareaId)
    {
        var tarea = await _tareaRepositorio.ObtenerPorId(tareaId);
        if (tarea is not null)
            await _tareaRepositorio.Eliminar(tarea);
    }
}
