using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Resguardo.Application.Commands;
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
using Resguardo.Application.Queries.ListarConfig;
using Resguardo.Application.Queries.ListarEfectivos;
using Resguardo.Application.Queries.ListarServicio;
using Resguardo.Application.Queries.ListarServicioProv;
using Resguardo.Application.Queries.ObtenerPersonal;
using Resguardo.Application.Queries.ObtenerSolicitud;
using Resguardo.Application.Services;
using Resguardo.Domain.Interfaces;
using Resguardo.Infrastructure.Data;
using Resguardo.Infrastructure.QueryServices;
using Resguardo.Infrastructure.Repositorios;
using Resguardo.Infrastructure.Services;
using Resguardo.Web.Authorization;
using Resguardo.Web.Handler;
using Resguardo.Web.Middlewares;
using Serilog;
using Serilog.Events;

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
            path: "Logs/rpo-web-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 14
        ).CreateLogger();
builder.Host.UseSerilog();

var mvcBuilder = builder.Services.AddControllersWithViews();
builder.Services.AddValidatorsFromAssemblyContaining<RegistrarSolicitudValidator>();

if (builder.Environment.IsDevelopment())
    mvcBuilder.AddRazorRuntimeCompilation();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContextFactory<DBContexto>(
    dbContextOptions => dbContextOptions
        .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
        .EnableSensitiveDataLogging()
        .EnableDetailedErrors()
        .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information)
);
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddScoped<IUnidadTrabajo, UnidadTrabajo>();
builder.Services.AddScoped(typeof(IRepositorioBase<>), typeof(RepositorioBase<>));
builder.Services.AddScoped<IUsuarioContexto, UsuarioContexto>();
builder.Services.AddScoped<ISolicitudQueryService, SolicitudQueryService>();
builder.Services.AddScoped<IGenericoQueryService, GenericoQueryService>();
builder.Services.AddScoped<IServicioQueryService, ServicioQueryService>();
builder.Services.AddScoped<IServicioProvQueryService, ServicioProvQueryService>();
builder.Services.AddScoped<IEfectivoQueryService, EfectivoQueryService>();
builder.Services.AddScoped<IPersonalQueryService, PersonalQueryService>();
builder.Services.AddScoped<IConfigQueryService, ConfigQueryService>();
builder.Services.AddScoped<IValidacionService, ValidacionService>();
builder.Services.AddScoped<RegistrarSolicitudHandler>();
builder.Services.AddScoped<AprobarSolicitudHandler>();
builder.Services.AddScoped<ActualizarSolicitudHandler>();
builder.Services.AddScoped<ConfirmarServicioHandler>();
builder.Services.AddScoped<ConsultarSolicitudHandler>();
builder.Services.AddScoped<ConsultarServicioHandler>();
builder.Services.AddScoped<ObtenerSolicitudHandler>();
builder.Services.AddScoped<ListarGenericoHandler>();
builder.Services.AddScoped<ListarServicioHandler>();
builder.Services.AddScoped<ListarServicioProvHandler>();
builder.Services.AddScoped<ListarEfectivoHandler>();
builder.Services.AddScoped<AsignarEfectivoHandler>();
builder.Services.AddScoped<CerrarServicioHandler>();
builder.Services.AddScoped<ObtenerPersonalHandler>();
builder.Services.AddScoped<ListarConfigHandler>();
builder.Services.AddScoped<RegistrarConfigHandler>();
builder.Services.AddScoped<CopiarConfigHandler>();
builder.Services.AddScoped<EditarSolicitudHandler>();
builder.Services.AddScoped<AmpliarServicioHandler>();
builder.Services.AddScoped<AprobarAmpliaHandler>();
builder.Services.AddSingleton<IMsalHttpClientFactory, NoProxyMsalHttpClientFactory>();
builder.Services.AddSingleton<TokenService>();
builder.Services.AddHttpClient<ISytelineService, SytelineService>();
builder.Services.AddHttpClient<IMaestroService, MaestroService>();
builder.Services.AddHttpClient<ISeguridadService, SeguridadService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication("Internal").AddScheme<AuthenticationSchemeOptions, InternalAuthHandler>("Internal", null);
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Permission", policy => policy.Requirements.Add(new PermissionRequirement("DUMMY")));
});
builder.Services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error/Error");    
    app.UseHsts();
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseStatusCodePagesWithReExecute("/Error/Error", "?code={0}");
app.UseHttpsRedirection();
app.UsePathBase("/resguardo");
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.Use(async (context, next) =>
{
    if (!context.Request.Headers.ContainsKey("X-User-Oid"))
    {
        context.Response.StatusCode = 403;
        return;
    }
    await next();
});
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=SolicitudVisualizar}/{action=Consulta}/{id?}");
app.Run();
