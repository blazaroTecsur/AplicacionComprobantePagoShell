using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.TokenCacheProviders.InMemory;
using Microsoft.Identity.Web.UI;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using Shell.Web.Helpers;
using Shell.Web.Middleware;
using Shell.Web.Services;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
   .SetBasePath(Directory.GetCurrentDirectory())
   .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
   .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
   .AddEnvironmentVariables();

Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
        .Enrich.FromLogContext()
        .Enrich.WithEnvironmentName()
        .Enrich.WithThreadId()
        .WriteTo.Console()
        .WriteTo.File(
            path: "Logs/she-web-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 14
        ).CreateLogger();
builder.Host.UseSerilog();

var mvcBuilder = builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<NoCacheFilter>();
}).AddMicrosoftIdentityUI();
if (builder.Environment.IsDevelopment())
    mvcBuilder.AddRazorRuntimeCompilation();

builder.Services.AddSingleton<IMsalHttpClientFactory, NoProxyMsalHttpClientFactory>();
builder.Services.AddHttpClient<ApiService>();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = Constante.SCHEMA_CORPORATE;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddOpenIdConnect(Constante.SCHEMA_CORPORATE, options =>
    {
        ConfigureOpenId(
            options,
            builder.Configuration.GetSection($"AzureAd:{Constante.SCHEMA_CORPORATE}"),
            builder.Configuration,
            Constante.SCHEMA_CORPORATE);
        options.BackchannelHttpHandler =
        new HttpClientHandler
        {
            UseProxy = false,
            Proxy = null
        };
    })
    .AddOpenIdConnect(Constante.SCHEMA_EXTERNAL, options =>
    {
        ConfigureOpenId(
            options,
            builder.Configuration.GetSection($"AzureAd:{Constante.SCHEMA_EXTERNAL}"),
            builder.Configuration,
            Constante.SCHEMA_EXTERNAL);
        options.BackchannelHttpHandler =
        new HttpClientHandler
        {
            UseProxy = false,
            Proxy = null
        };
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddTokenAcquisition();
builder.Services.AddInMemoryTokenCaches();

void ConfigureOpenId(
    OpenIdConnectOptions options,
    IConfigurationSection section,
    IConfiguration configuration,
    string schema)
{
    options.Authority = section["Authority"];
    options.MetadataAddress = $"{section["Authority"]}/v2.0/.well-known/openid-configuration";
    options.ClientId = section["ClientId"];
    options.ClientSecret = section["ClientSecret"];
    options.CallbackPath = section["CallbackPath"];
    options.ResponseType = "code";
    options.ResponseMode = "form_post";
    options.UsePkce = true;
    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = false;
    options.MapInboundClaims = false;

    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("offline_access");
    options.Scope.Add(configuration[$"ApiSettings:Scope{schema}"]);
    options.BackchannelHttpHandler = new HttpClientHandler
    {
        UseProxy = false,
        Proxy = null
    };
    options.TokenValidationParameters =
        new TokenValidationParameters
        {
            ValidateIssuer = false,
            NameClaimType = "name",
            RoleClaimType = "roles"
        };

    options.Events = new OpenIdConnectEvents
    {
        OnTokenValidated = async context =>
        {
            var identity = (ClaimsIdentity)context.Principal!.Identity!;
            if (!identity.HasClaim(c => c.Type == "session_id"))
                identity.AddClaim(new Claim("session_id", Guid.NewGuid().ToString()));
            if (!identity.HasClaim(c => c.Type == "auth_scheme"))
                identity.AddClaim(new Claim("auth_scheme", context.Scheme.Name));

            // Obtener NomTenant desde la API y guardarlo como claim empresa en la cookie.
            // Esto evita hardcodear tenant→empresa en los módulos del shell.
            try
            {
                var accessToken = context.TokenEndpointResponse?.AccessToken;
                if (!string.IsNullOrEmpty(accessToken))
                {
                    var api = context.HttpContext.RequestServices.GetRequiredService<ApiService>();
                    var usuario = await api.ObtenerUsuario(accessToken);
                    if (!string.IsNullOrEmpty(usuario.NomTenant))
                        identity.AddClaim(new Claim("empresa", usuario.NomTenant));
                }
            }
            catch { /* no bloquear el login si la API falla */ }
        },
        OnRedirectToIdentityProviderForSignOut = context =>
        {
            context.ProtocolMessage.PostLogoutRedirectUri = configuration["AppBaseUrl"];
            return Task.CompletedTask;
        },
        OnRedirectToIdentityProvider = context =>
        {
            //context.ProtocolMessage.Prompt = "login";
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            context.HandleResponse();
            context.Response.Redirect(
                "/Home/Error?message=" +
                Uri.EscapeDataString(
                    context.Exception.Message));
            return Task.CompletedTask;
        }
    };
}
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
builder.Services.Configure<CookieAuthenticationOptions>(
    CookieAuthenticationDefaults.AuthenticationScheme,
    options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor
});
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=SelectTenant}/{id?}");
app.MapReverseProxy(proxyPipeline =>
{
    proxyPipeline.Use(async (context, next) =>
    {
        if (!(context.User?.Identity?.IsAuthenticated ?? false))
        {
            context.Response.Redirect("/Auth/SelectTenant");
            return;
        }

        var usuario = context.User;
        string? codTenant = usuario.FindFirst("tid")?.Value;
        string? codUsuario = usuario.FindFirst("oid")?.Value;
        string? nomUsuario = usuario.FindFirst("name")?.Value;
        string? usuCorreo = usuario.FindFirst("preferred_username")?.Value;
        string? idSesion = usuario.FindFirst("session_id")?.Value;
        string? schema = usuario.FindFirst("auth_scheme")?.Value;

        var empresa = usuario.FindFirst("empresa")?.Value;

        context.Request.Headers["X-User-Oid"] = codUsuario ?? "";
        context.Request.Headers["X-Tenant-Id"] = codTenant ?? "";
        context.Request.Headers["X-User-Name"] = nomUsuario ?? "";
        context.Request.Headers["X-User-Email"] = usuCorreo ?? "";
        context.Request.Headers["X-Session-Id"] = idSesion ?? "";
        context.Request.Headers["X-Schema"] = schema ?? "";
        context.Request.Headers["X-Empresa"] = empresa ?? "";

        await next();
    });
});
app.Run();
