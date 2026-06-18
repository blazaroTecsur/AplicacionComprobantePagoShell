using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Reciclaje.Aplicacion.Configuracion;
using Reciclaje.Aplicacion.Interfaces;
using Reciclaje.Aplicacion.DTOs;

namespace Reciclaje.Web.Controllers;

[ApiController]
[Route("api/syteline")]
public class SyteLineController(
    ISyteLineServicio servicio,
    IOptions<SyteLineConfig> options) : ControllerBase
{
    private readonly SyteLineConfig _cfg = options.Value;

    // ── POST api/syteline/run ────────────────────────────────────────────────
    [HttpPost("run")]
    public async Task<IActionResult> Run([FromBody] RunRequest request)
    {
        try
        {
            // 1. Obtener token OAuth2 usando los valores de configuración tipada
            var tokenEl = await servicio.GetTokenAsync(
                accessTokenUrl: _cfg.AccessTokenUrl,
                clientId: _cfg.ClientId,
                clientSecret: _cfg.ClientSecret,
                username: _cfg.Username,
                password: _cfg.Password);

            if (!tokenEl.TryGetProperty("access_token", out var tokenProp))
                return BadRequest(new { error = "No se pudo obtener el token de SyteLine" });

            var accessToken = tokenProp.GetString()!;
            var mongooseConfig = _cfg.MongooseConfig;

            // 2. GET IDO — FSSROMatls
            var campos = "SroNum,SroLine,SroOper,RowPointer,SiteSite,TransNum,TransDate,Item,Whse,UM,MatlQty,Posted,Dept";
            var filtro = string.IsNullOrWhiteSpace(request.SroNum)
                ? string.Empty
                : $"?filter=SroNum='{request.SroNum}'%20AND%20Type='P'";
            var endpointUrl = $"{_cfg.IdoBaseUrl}/FSSROMatls/{Uri.EscapeDataString(campos)}/adv{filtro}";

            var idoResult = await servicio.IdoGetAsync(endpointUrl, accessToken, mongooseConfig);

            if (!idoResult.TryGetProperty("Items", out var itemsEl) ||
                itemsEl.ValueKind != JsonValueKind.Array)
                return Ok(new { success = true, mensaje = "Sin líneas para procesar", items = 0 });

            // Convertir estructura [{Name,Value}] a objeto JSON plano
            var itemsList = itemsEl.EnumerateArray()
                .Select(fila =>
                {
                    var dict = fila.EnumerateArray()
                        .ToDictionary(
                            p => p.GetProperty("Name").GetString()!,
                            p => p.GetProperty("Value").GetString() ?? string.Empty);
                    return JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(dict));
                })
                .ToList();

            if (itemsList.Count == 0)
                return Ok(new { success = true, mensaje = "Sin líneas para procesar", items = 0 });

            // 3. INSERT integracionsyteline
            var (ins, skip, errs) = await servicio.InsertIntegracionAsync(itemsList);

            // 4. INSERT sro + srolinea
            var resultSro = await servicio.InsertSroAsync(itemsList);

            return Ok(new
            {
                success = true,
                totalItems = itemsList.Count,
                integracion = new { insertados = ins, omitidos = skip, errores = errs },
                sro = resultSro
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // ── POST api/syteline/token ──────────────────────────────────────────────
    [HttpPost("token")]
    public async Task<IActionResult> Token([FromBody] TokenRequest r) =>
        Ok(await servicio.GetTokenAsync(r.AccessTokenUrl, r.ClientId, r.ClientSecret,
                                        r.Username, r.Password));

    // ── POST api/syteline/get ────────────────────────────────────────────────
    [HttpPost("get")]
    public async Task<IActionResult> Get([FromBody] IdoRequest r) =>
        Ok(await servicio.IdoGetAsync(r.EndpointUrl, r.AccessToken, r.MongooseConfig));

    // ── POST api/syteline/updatepo ───────────────────────────────────────────
    [HttpPost("updatepo")]
    public async Task<IActionResult> UpdatePo([FromBody] IdoPutRequest r) =>
        Ok(await servicio.IdoPutAsync(r.EndpointUrl, r.AccessToken, r.MongooseConfig, r.Body));

    // ── POST api/syteline/mysql/insert ──────────────────────────────────────
    [HttpPost("mysql/insert")]
    public async Task<IActionResult> Insert([FromBody] ItemsRequest r)
    {
        if (r.Items is not { Length: > 0 }) return BadRequest("items vacío");
        var (ins, skip, errs) = await servicio.InsertIntegracionAsync(r.Items);
        return Ok(new { success = true, insertados = ins, omitidos = skip, errores = errs });
    }

    // ── POST api/syteline/mysql/insertsro ───────────────────────────────────
    [HttpPost("mysql/insertsro")]
    public async Task<IActionResult> InsertSro([FromBody] ItemsRequest r)
    {
        if (r.Items is not { Length: > 0 }) return BadRequest("items vacío");
        return Ok(await servicio.InsertSroAsync(r.Items));
    }

    // ── POST api/syteline/mysql/buildpayload ─────────────────────────────────
    [HttpPost("mysql/buildpayload")]
    public async Task<IActionResult> BuildPayload([FromBody] BuildPayloadRequest? r) =>
        Ok(await servicio.BuildPayloadAsync(r?.SroLineaIds));
}
