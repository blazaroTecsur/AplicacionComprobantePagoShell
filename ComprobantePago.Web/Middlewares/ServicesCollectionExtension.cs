using ComprobantePago.Application.Common;
using ComprobantePago.Application.Interfaces;
using ComprobantePago.Application.Interfaces.QueryServices;
using ComprobantePago.Application.Interfaces.Repositories;
using ComprobantePago.Application.Interfaces.Services;
using ComprobantePago.Application.Interfaces.Services.Maestros;
using ComprobantePago.Application.Mapping;
using ComprobantePago.Application.Validations;
using ComprobantePago.Infrastructure.Persistence;
using ComprobantePago.Infrastructure.QueryServices;
using ComprobantePago.Infrastructure.Repositories;
using ComprobantePago.Infrastructure.Services;
using ComprobantePago.Infrastructure.Services.Maestros;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Notificacion.Infrastructure.DependencyInjection;
using Seguridad.Infrastructure.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using System.Text.Json;

namespace ComprobantePago.Web.Middlewares
{
    public static class ServicesCollectionExtension
    {
        public static WebApplicationBuilder AddSeriLog(this WebApplicationBuilder builder)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
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

            builder.Host.UseSerilog();
            return builder;
        }

        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseMySql(
                    config.GetConnectionString("DefaultConnection"),
                    new MySqlServerVersion(new Version(8, 0, 0))
                )
            );
            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration config)
        {
            // MVC JSON
            services.AddControllersWithViews()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                });

            // Swagger
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new()
                {
                    Title       = "ComprobantePago API",
                    Version     = "v1",
                    Description = "API REST para la gestión de comprobantes de pago."
                });
            });

            // Settings
            services.Configure<SunatSettings>(config.GetSection("Sunat"));
            services.Configure<EmpresaSettings>(config.GetSection(EmpresaSettings.Section));
            services.Configure<ApiMaestrosSettings>(config.GetSection(ApiMaestrosSettings.Section));

            // HttpClient SUNAT
            services.AddHttpClient<ISunatService, SunatService>()
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    var handler = new HttpClientHandler { UseProxy = false };
                    return handler;
                });

            // Mappings y validaciones
            MapsterConfig.Configure();
            services.AddValidatorsFromAssemblyContaining<RegistrarComprobanteValidator>();

            // Maestros: API o BD según configuración
            var usarApiMaestros = config.GetValue<bool>($"{ApiMaestrosSettings.Section}:UsarApi");
            if (usarApiMaestros)
            {
                services.AddHttpClient<IEmpleadoService, ApiEmpleadoService>();
                services.AddHttpClient<IProveedorService, ApiProveedorService>();
                services.AddHttpClient<ICatalogoUnidadService, ApiCatalogoUnidadService>();
                services.AddHttpClient<ICuentaContableService, ApiCuentaContableService>();
            }
            else
            {
                services.AddScoped<IEmpleadoService, DbEmpleadoService>();
                services.AddScoped<IProveedorService, DbProveedorService>();
                services.AddScoped<ICatalogoUnidadService, DbCatalogoUnidadService>();
                services.AddScoped<ICuentaContableService, DbCuentaContableService>();
            }

            // Seguridad
            services.AddMemoryCache();
            services.AddHttpContextAccessor();
            services.AddSeguridad(config);

            // Infor / Syteline
            services.AddInfor(config);
            services.AddScoped<ISytelineEnvioService, SytelineEnvioService>();

            // Servicios de dominio
            services.AddScoped<XmlComprobanteService>();
            services.AddScoped<PdfComprobanteService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IComprobanteQueryService, ComprobanteQueryService>();
            services.AddScoped<ISytelineQueryService, SytelineQueryService>();
            services.AddScoped<IMaestrosQueryService, MaestrosQueryService>();
            services.AddScoped<IComprobanteRepository, ComprobanteRepository>();
            services.AddScoped<IExcelSytelineService, ExcelSytelineService>();

            return services;
        }
    }
}
