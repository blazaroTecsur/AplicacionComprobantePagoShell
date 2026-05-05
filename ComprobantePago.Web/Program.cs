using ComprobantePago.Application.Common;
using ComprobantePago.Application.Interfaces;
using ComprobantePago.Application.Interfaces.QueryServices;
using ComprobantePago.Application.Interfaces.Repositories;
using ComprobantePago.Application.Interfaces.Services;
using ComprobantePago.Application.Interfaces.Services.Maestros;
using ComprobantePago.Application.Mapping;
using ComprobantePago.Application.Settings;
using ComprobantePago.Application.Validations;
using ComprobantePago.Infrastructure.Persistence;
using ComprobantePago.Infrastructure.QueryServices;
using ComprobantePago.Infrastructure.Repositories;
using ComprobantePago.Infrastructure.Services;
using ComprobantePago.Infrastructure.Services.Maestros;
using ComprobantePago.Web.Middlewares;
using FluentValidation;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using System.Text.Json;

// ── Serilog: configurar antes del builder ─────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command",
        LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()

    // Consola: texto legible para desarrollo
    .WriteTo.Console(
        outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] [{CorrelationId}] {UserId} | {Message:lj}{NewLine}{Exception}")

    // Archivo texto: historial legible en producción
    .WriteTo.File(
        path: "logs/comprobante-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate:
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{CorrelationId}] {UserId} | {Message:lj}{NewLine}{Exception}")

    // Archivo JSON estructurado: consumible por sistemas de observabilidad
    // (Seq, Elastic, Splunk, etc.). Incluye RequestId, CorrelationId, UserId automáticamente.
    .WriteTo.File(
        formatter: new CompactJsonFormatter(),
        path: "logs/comprobante-json-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        restrictedToMinimumLevel: LogEventLevel.Information)

    // Archivo JSON solo para auditoría (filtro por propiedad AuditLog=true)
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(e =>
            e.Properties.ContainsKey("AuditLog") &&
            e.Properties["AuditLog"].ToString() == "True")
        .WriteTo.File(
            formatter: new CompactJsonFormatter(),
            path: "logs/audit-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 90))

    .CreateLogger();

try
{
    Log.Information("Iniciando aplicación ComprobantePago...");

    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog ───────────────────────────────────────────────────────────────
    builder.Host.UseSerilog();

    // ── MVC + JSON camelCase ──────────────────────────────────────────────────
    builder.Services.AddControllersWithViews()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy    = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        });

    // ── Swagger / OpenAPI ─────────────────────────────────────────────────────
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new()
        {
            Title       = "ComprobantePago API",
            Version     = "v1",
            Description = "API REST para la gestión de comprobantes de pago."
        });
    });

    // ── Base de datos MySQL ───────────────────────────────────────────────────
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseMySql(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            new MySqlServerVersion(new Version(8, 0, 0))
        )
    );

    // ── Configuración SUNAT ───────────────────────────────────────────────────
    builder.Services.Configure<SunatSettings>(
        builder.Configuration.GetSection("Sunat"));

    // ── Configuración Empresa ─────────────────────────────────────────────────
    builder.Services.Configure<EmpresaSettings>(
        builder.Configuration.GetSection(EmpresaSettings.Section));

    // ── HttpClient SUNAT ──────────────────────────────────────────────────────
    builder.Services.AddHttpClient<ISunatService, SunatService>()
        .ConfigurePrimaryHttpMessageHandler(() =>
        {
            var handler = new HttpClientHandler { UseProxy = false };
            // Omitir validación SSL solo en desarrollo (SUNAT Beta usa cert autofirmado).
            // En producción se valida el certificado normalmente.
            if (builder.Environment.IsDevelopment())
            {
                handler.ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            }
            return handler;
        });

    // ── Mapster: configurar mappings de la capa Application ──────────────────
    MapsterConfig.Configure();

    // ── FluentValidation ──────────────────────────────────────────────────────
    builder.Services.AddValidatorsFromAssemblyContaining<RegistrarComprobanteValidator>();

    // ── Configuración ApiMaestros ─────────────────────────────────────────────
    builder.Services.Configure<ApiMaestrosSettings>(
        builder.Configuration.GetSection(ApiMaestrosSettings.Section));

    var usarApiMaestros = builder.Configuration
        .GetValue<bool>($"{ApiMaestrosSettings.Section}:UsarApi");

    if (usarApiMaestros)
    {
        builder.Services.AddHttpClient<IEmpleadoService, ApiEmpleadoService>();
        builder.Services.AddHttpClient<IProveedorService, ApiProveedorService>();
        builder.Services.AddHttpClient<ICatalogoUnidadService, ApiCatalogoUnidadService>();
        builder.Services.AddHttpClient<ICuentaContableService, ApiCuentaContableService>();
    }
    else
    {
        builder.Services.AddScoped<IEmpleadoService, DbEmpleadoService>();
        builder.Services.AddScoped<IProveedorService, DbProveedorService>();
        builder.Services.AddScoped<ICatalogoUnidadService, DbCatalogoUnidadService>();
        builder.Services.AddScoped<ICuentaContableService, DbCuentaContableService>();
    }

    // ── Autenticación JWT / Azure Entra ID (multi-tenant) ────────────────────
    // Microsoft.Identity.Web valida automáticamente: firma, expiración, audiencia,
    // emisor y nonce (requisitos 3.2 del spec OIDC/PKCE interno).
    // En producción: completar ClientId, Audience y ValidTenants en appsettings.
    var azureAdConfig = builder.Configuration.GetSection("AzureAd");
    // Filtra strings vacíos para que [""] (valor por defecto en dev) se trate igual que [].
    var validTenants  = (azureAdConfig.GetSection("ValidTenants").Get<string[]>() ?? [])
                        .Where(t => !string.IsNullOrWhiteSpace(t))
                        .ToArray();
    var esDesarrollo  = validTenants.Length == 0;

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<IUsuarioContexto, UsuarioContexto>();

    // AddMicrosoftIdentityWebApiAuthentication lee la sección "AzureAd" y configura
    // JwtBearer con toda la validación OIDC requerida por el spec 3.2.
    // En desarrollo (ValidTenants vacío / ClientId sin configurar) se registra
    // un JwtBearer mínimo sin validación para no bloquear el arranque local.
    if (esDesarrollo)
    {
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();
    }
    else
    {
        builder.Services.AddMicrosoftIdentityWebApiAuthentication(
            builder.Configuration, configSectionName: "AzureAd");

        // Control de tenants (spec 3.3): en producción solo se aceptan los GUIDs
        // de tenant configurados en ValidTenants.
        builder.Services.PostConfigure<JwtBearerOptions>(
            JwtBearerDefaults.AuthenticationScheme,
            options =>
            {
                var anteriorOnTokenValidated = options.Events?.OnTokenValidated;
                options.Events ??= new JwtBearerEvents();
                options.Events.OnTokenValidated = async ctx =>
                {
                    // Encadenar el handler que Microsoft.Identity.Web ya registró
                    if (anteriorOnTokenValidated is not null)
                        await anteriorOnTokenValidated(ctx);

                    var tid = ctx.Principal?.FindFirst("tid")?.Value
                           ?? ctx.Principal?.FindFirst(
                                  "http://schemas.microsoft.com/identity/claims/tenantid")?.Value;

                    if (string.IsNullOrEmpty(tid) || !validTenants.Contains(tid))
                        ctx.Fail($"Tenant no autorizado: {tid}");
                };
            });
    }

    // ── Autorización basada en App Roles de Azure Entra ID ───────────────────
    // Roles definidos en el manifest de la app registrada:
    //   Digitador · Autorizador · Aprobador · Anulador
    //
    // En desarrollo (ValidTenants vacío) todas las políticas se resuelven como
    // autorizadas automáticamente para no bloquear el flujo local sin token JWT.
    builder.Services.AddAuthorization(options =>
    {
        if (esDesarrollo)
        {
            var todoPermitido = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                .RequireAssertion(_ => true)
                .Build();
            options.DefaultPolicy  = todoPermitido;
            options.FallbackPolicy = null;
            options.AddPolicy("RequiereDigitador",   todoPermitido);
            options.AddPolicy("RequiereAutorizador", todoPermitido);
            options.AddPolicy("RequiereAprobador",   todoPermitido);
            options.AddPolicy("RequiereAnulador",    todoPermitido);
        }
        else
        {
            // Producción: el token JWT debe incluir el claim "roles" con el valor correcto.
            options.AddPolicy("RequiereDigitador",   p => p.RequireAuthenticatedUser()
                .RequireClaim("roles", "Digitador"));
            options.AddPolicy("RequiereAutorizador", p => p.RequireAuthenticatedUser()
                .RequireClaim("roles", "Autorizador"));
            options.AddPolicy("RequiereAprobador",   p => p.RequireAuthenticatedUser()
                .RequireClaim("roles", "Aprobador"));
            options.AddPolicy("RequiereAnulador",    p => p.RequireAuthenticatedUser()
                .RequireClaim("roles", "Anulador"));
        }
    });

    // ── Infor Syteline IDO REST API ───────────────────────────────────────────
    builder.Services.Configure<InforSettings>(
        builder.Configuration.GetSection(InforSettings.Section));

    // HttpClient nombrado para el token service (sin typed client para permitir Singleton)
    builder.Services.AddHttpClient(nameof(InforTokenService));

    // Singleton: el caché del token (SemaphoreSlim + campo privado) debe vivir
    // durante toda la vida de la aplicación para reutilizarse entre requests.
    builder.Services.AddSingleton<IInforTokenService>(sp =>
        new InforTokenService(
            sp.GetRequiredService<IHttpClientFactory>()
              .CreateClient(nameof(InforTokenService)),
            sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<InforSettings>>(),
            sp.GetRequiredService<ILogger<InforTokenService>>()));

    // Typed HttpClient para el servicio IDO (resuelve IInforTokenService desde DI)
    builder.Services.AddHttpClient<ISytelineIdoService, SytelineIdoService>();
    builder.Services.AddScoped<ISytelineEnvioService, SytelineEnvioService>();

    // ── Servicios de aplicación ───────────────────────────────────────────────
    builder.Services.AddScoped<XmlComprobanteService>();
    builder.Services.AddScoped<PdfComprobanteService>();
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddScoped<IComprobanteQueryService, ComprobanteQueryService>();
    builder.Services.AddScoped<ISytelineQueryService, SytelineQueryService>();
    builder.Services.AddScoped<IMaestrosQueryService, MaestrosQueryService>();
    builder.Services.AddScoped<IComprobanteRepository, ComprobanteRepository>();
    builder.Services.AddScoped<IExcelSytelineService, ExcelSytelineService>();

    var app = builder.Build();

    // ── Columnas pendientes de BD (idempotente, IF NOT EXISTS) ────────────────
    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.ExecuteSqlRawAsync(@"
            ALTER TABLE rcocomprobante
                ADD COLUMN IF NOT EXISTS FechaDigitacion   DATETIME NULL,
                ADD COLUMN IF NOT EXISTS FechaAutorizacion DATETIME NULL");
    }
    catch (Exception ex)
    {
        Log.Warning(ex,
            "No se pudo ejecutar la migración de columnas al iniciar. " +
            "Se reintentará en el siguiente arranque.");
    }

    // ── Pipeline de middlewares ───────────────────────────────────────────────
    // 1. Manejo global de excepciones (primero, captura todo)
    app.UseMiddleware<ExceptionMiddleware>();

    // 2. CorrelationId: inyecta CorrelationId, RequestId y UserId en LogContext
    //    antes de que cualquier log de la petición sea emitido.
    app.UseMiddleware<CorrelationIdMiddleware>();

    // 3. Serilog request logging (ya enriquecido con CorrelationId / UserId)
    app.UseSerilogRequestLogging(opts =>
    {
        opts.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} respondió {StatusCode} en {Elapsed:0.0000} ms";
    });

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    // 3. Swagger UI (disponible en todos los entornos)
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ComprobantePago API v1");
        c.RoutePrefix = "swagger";
    });

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    // 4. AuditMiddleware: audita POST críticos DESPUÉS de autenticar al usuario
    app.UseMiddleware<AuditMiddleware>();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Comprobante}/{action=Index}/{id?}");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación terminó inesperadamente.");
}
finally
{
    Log.CloseAndFlush();
}
