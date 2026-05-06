using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Notificacion.Abstractions;
using Notificacion.Application;
using Notificacion.Infrastructure.DependencyInjection;
using Notificacion.Infrastructure.Email;
using Resguardo.Application.Commands.ActualizarSolicitud;
using Resguardo.Application.Commands.AmpliarServicio;
using Resguardo.Application.Commands.AprobarAmplia;
using Resguardo.Application.Commands.AprobarSolicitud;
using Resguardo.Application.Commands.AsignarEfectivo;
using Resguardo.Application.Commands.CerrarServicio;
using Resguardo.Application.Commands.ConfirmarServicio;
using Resguardo.Application.Commands.CopiarConfig;
using Resguardo.Application.Commands.EditarSolicitud;
using Resguardo.Application.Commands.RegistrarConfig;
using Resguardo.Application.Commands.RegistrarSolicitud;
using Resguardo.Application.Common.Interfaces;
using Resguardo.Application.Common.Services;
using Resguardo.Application.Interfaces;
using Resguardo.Application.Queries.ConsultarSolicitud;
using Resguardo.Application.Queries.ListarEfectivos;
using Resguardo.Application.Queries.ListarLimites;
using Resguardo.Application.Queries.ListarServicio;
using Resguardo.Application.Queries.ListarServicioProv;
using Resguardo.Application.Queries.ObtenerConfig;
using Resguardo.Application.Queries.ObtenerPersonal;
using Resguardo.Application.Queries.ObtenerSolicitud;
using Resguardo.Application.Queries.ObtenerSolicitudFolio;
using Resguardo.Application.Queries.ReporteEfectivo;
using Resguardo.Application.Queries.ReporteSolicitud;
using Resguardo.Application.Services;
using Resguardo.Domain.Interfaces;
using Resguardo.Infrastructure.Background.Email;
using Resguardo.Infrastructure.Data;
using Resguardo.Infrastructure.QueryServices;
using Resguardo.Infrastructure.Repositorios;
using Resguardo.Infrastructure.Services;
using Seguridad.Infrastructure.DependencyInjection;
using Serilog;
using Serilog.Events;

namespace Resguardo.Web.Middlewares
{
    public static class ServicesCollectionExtension
    {
        public static WebApplicationBuilder AddSettings(this WebApplicationBuilder builder, IConfiguration config)
        {
            builder.Configuration
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
               .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
               .AddEnvironmentVariables();
            return builder;
        }
        public static WebApplicationBuilder AddSeriLog(this WebApplicationBuilder builder, IConfiguration config)
        {
            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithEnvironmentName()
            .Enrich.WithThreadId()
            .WriteTo.Console()
            .WriteTo.File(
                path: "Logs/rpo-web-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14
            ).CreateLogger();
            builder.Host.UseSerilog();
            return builder;
        }
        public static IServiceCollection AddConnectionDataBase(this IServiceCollection services, IConfiguration config)
        {
            var connectionString = config.GetConnectionString("DefaultConnection");
            services.AddDbContextFactory<DBContexto>(
                dbContextOptions => dbContextOptions
                    .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors()
                    .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information)
            );
            return services;
        }
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddHttpContextAccessor();
            services.AddValidatorsFromAssemblyContaining<RegistrarSolicitudValidator>();            
            services.AddEmail(config);
            services.AddScoped<IEmailService, SmtpEmailService>();
            services.AddScoped<ITemplateService, TemplateService>();
            services.AddSingleton<EmailQueue>();
            services.AddSingleton<IEmailQueue>(sp => sp.GetRequiredService<EmailQueue>());
            services.AddHostedService<EmailBackgroundService>();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddScoped<IUnidadTrabajo, UnidadTrabajo>();
            services.AddScoped(typeof(IRepositorioBase<>), typeof(RepositorioBase<>));
            services.AddScoped<ISolicitudQueryService, SolicitudQueryService>();
            services.AddScoped<IGenericoQueryService, GenericoQueryService>();
            services.AddScoped<IServicioQueryService, ServicioQueryService>();
            services.AddScoped<IServicioProvQueryService, ServicioProvQueryService>();
            services.AddScoped<IEfectivoQueryService, EfectivoQueryService>();
            services.AddScoped<IPersonalQueryService, PersonalQueryService>();
            services.AddScoped<ILimiteQueryService, LimiteQueryService>();
            services.AddScoped<IReporteQueryService, ReporteQueryService>();
            services.AddScoped<IValidacionService, ValidacionService>();
            services.AddScoped<RegistrarSolicitudHandler>();
            services.AddScoped<AprobarSolicitudHandler>();
            services.AddScoped<ActualizarSolicitudHandler>();
            services.AddScoped<ConfirmarServicioHandler>();
            services.AddScoped<ConsultarSolicitudHandler>();
            services.AddScoped<ConsultarServicioHandler>();
            services.AddScoped<ObtenerSolicitudHandler>();
            services.AddScoped<ListarGenericoHandler>();
            services.AddScoped<ListarServicioHandler>();
            services.AddScoped<ListarServicioProvHandler>();
            services.AddScoped<ListarEfectivoHandler>();
            services.AddScoped<AsignarEfectivoHandler>();
            services.AddScoped<CerrarServicioHandler>();
            services.AddScoped<ObtenerPersonalHandler>();
            services.AddScoped<ListarLimitesHandler>();
            services.AddScoped<ObtenerLimitesHandler>();
            services.AddScoped<RegistrarConfigHandler>();
            services.AddScoped<CopiarConfigHandler>();
            services.AddScoped<EditarSolicitudHandler>();
            services.AddScoped<AmpliarServicioHandler>();
            services.AddScoped<AprobarAmpliaHandler>();
            services.AddScoped<ReporteSolicitudHandler>();
            services.AddScoped<ReporteEfectivoHandler>();
            services.AddScoped<ObtenerSolicitudFolioHandler>();
            services.AddInfor(config);
            services.AddSeguridad(config);
            services.AddScoped<IInforService, InforService>();
            services.AddHttpClient<IMaestroService, MaestroService>();
            services.AddMemoryCache();

            return services;
        }
    }
}
