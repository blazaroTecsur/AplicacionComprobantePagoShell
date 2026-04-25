using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using Shell.Web.Middleware;
using Shell.Web.Services;
using System.Data;
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
    .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(options =>
    {
        builder.Configuration.Bind("AzureAd", options);

        //options.Authority = "https://login.microsoftonline.com/common/v2.0";
        options.ResponseType = "code";
        options.SaveTokens = true;
        options.MapInboundClaims = false;

        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("offline_access");
        options.Scope.Add(builder.Configuration["ApiSettings:Scope"]);
        options.BackchannelHttpHandler = new HttpClientHandler
        {
            UseProxy = false,
            Proxy = null
        };
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            NameClaimType = "name",
            RoleClaimType = "roles"
        };

        options.Events = new OpenIdConnectEvents
        {
            OnTokenValidated = context =>
            {
                var identity = (ClaimsIdentity)context.Principal!.Identity!;

                if (context.Properties.Items.TryGetValue("tenant", out var tenant))
                    identity.AddClaim(new Claim("tenant", tenant));
                if (!identity.HasClaim(c => c.Type == "session_id"))
                    identity.AddClaim(new Claim("session_id", Guid.NewGuid().ToString()));

                return Task.CompletedTask;
            },

            OnRedirectToIdentityProvider = context =>
            {
                if (context.Properties.Items.TryGetValue("tenant", out var selectedTenant)
                 && !string.IsNullOrWhiteSpace(selectedTenant))
                {
                    var tenantId = builder.Configuration[$"Tenants:{selectedTenant}:TenantId"];

                    context.ProtocolMessage.IssuerAddress =
                        $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize";
                }

                return Task.CompletedTask;
            },

            OnRedirectToIdentityProviderForSignOut = context =>
            {
                context.ProtocolMessage.PostLogoutRedirectUri = builder.Configuration["AppBaseUrl"];
                return Task.CompletedTask;
            }
        };
    })
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddInMemoryTokenCaches();

builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
//builder.Services.AddAuthorization(options =>
//{fv
//    options.AddPolicy("TicketAccess", policy =>
//        policy.RequireClaim("roles", "InventarioUser"));
//});
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
        string ObtenerClaim(params string[] tipo) => tipo
                .Select(t => usuario.FindFirst(t)?.Value)
                .FirstOrDefault(v => !string.IsNullOrWhiteSpace(v)) ?? string.Empty;

        string usuCorreo = ObtenerClaim("preferred_username");
        string codTenant = ObtenerClaim("tid", "http://schemas.microsoft.com/identity/claims/tenantid");
        string codUsuario = ObtenerClaim("oid", "http://schemas.microsoft.com/identity/claims/objectidentifier");
        string idSesion = ObtenerClaim("session_id");

        context.Request.Headers["X-User-Email"] = usuCorreo ?? "";
        context.Request.Headers["X-User-Oid"] = codUsuario ?? "";
        context.Request.Headers["X-Tenant-Id"] = codTenant ?? "";
        context.Request.Headers["X-Session-Id"] = idSesion ?? "";

        await next();
    });
});
app.Run();
