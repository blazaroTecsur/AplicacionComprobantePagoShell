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
using Microsoft.EntityFrameworkCore;
using Seguridad.Infrastructure.DependencyInjection;
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
    .WriteTo.Console(
        outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] [{CorrelationId}] {UserId} | {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/comprobante-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate:
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{CorrelationId}] {UserId} | {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        formatter: new CompactJsonFormatter(),
        path: "logs/comprobante-json-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        restrictedToMinimumLevel: LogEventLevel.Information)
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

    // ── Seguridad: autenticación, autorización y permisos ────────────────────
    builder.Services.AddMemoryCache();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddSeguridad(builder.Configuration);

    // ── Infor / Syteline ──────────────────────────────────────────────────────
    builder.Services.Configure<InforSettings>(
        builder.Configuration.GetSection(InforSettings.Section));

    builder.Services.AddHttpClient(nameof(InforTokenService));

    builder.Services.AddSingleton<IInforTokenService>(sp =>
        new InforTokenService(
            sp.GetRequiredService<IHttpClientFactory>()
              .CreateClient(nameof(InforTokenService)),
            sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<InforSettings>>(),
            sp.GetRequiredService<ILogger<InforTokenService>>()));

    builder.Services.AddHttpClient<ISytelineIdoService, SytelineIdoService>();
    builder.Services.AddScoped<ISytelineEnvioService, SytelineEnvioService>();

    // ── Servicios de dominio ──────────────────────────────────────────────────
    builder.Services.AddScoped<XmlComprobanteService>();
    builder.Services.AddScoped<PdfComprobanteService>();
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddScoped<IComprobanteQueryService, ComprobanteQueryService>();
    builder.Services.AddScoped<ISytelineQueryService, SytelineQueryService>();
    builder.Services.AddScoped<IMaestrosQueryService, MaestrosQueryService>();
    builder.Services.AddScoped<IComprobanteRepository, ComprobanteRepository>();
    builder.Services.AddScoped<IExcelSytelineService, ExcelSytelineService>();

    var app = builder.Build();

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

    app.UseMiddleware<ExceptionMiddleware>();
    app.UseMiddleware<CorrelationIdMiddleware>();

    app.UseSerilogRequestLogging(opts =>
    {
        opts.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} respondió {StatusCode} en {Elapsed:0.0000} ms";
    });

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ComprobantePago API v1");
        c.RoutePrefix = "swagger";
    });

    app.UsePathBase("/comprobante");
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();

    app.Use(async (context, next) =>
    {
        var user = context.User;
        if (!(user?.Identity?.IsAuthenticated ?? false))
        {
            context.Response.StatusCode = 401;
            return;
        }
        await next();
    });

    app.UseAuthorization();
    app.UseMiddleware<AuditMiddleware>();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=ComprobanteConsultar}/{action=Index}/{id?}");

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
