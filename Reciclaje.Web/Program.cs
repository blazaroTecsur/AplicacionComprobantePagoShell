using QuestPDF.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Reciclaje.Aplicacion.Configuracion;
using Reciclaje.Aplicacion.Interfaces;
using Reciclaje.Aplicacion.Servicios;
using Reciclaje.Dominio.Entidades;
using Reciclaje.Dominio.Interfaces;
using Reciclaje.Infraestructura.Data;
using Reciclaje.Infraestructura.Repositorios;

var builder = WebApplication.CreateBuilder(args);

// ── MVC ───────────────────────────────────────────────────────────────────────
var mvcBuilder = builder.Services.AddControllersWithViews();
if (builder.Environment.IsDevelopment())
    mvcBuilder.AddRazorRuntimeCompilation();

// ── Configuración tipada SyteLine ─────────────────────────────────────────────
// Registra y valida la sección "SyteLine" al arrancar la aplicación.
// Si falta algún campo [Required] (credenciales vacías), la app falla
// inmediatamente con un mensaje claro en lugar de explotar en tiempo de ejecución.
builder.Services
    .AddOptions<SyteLineConfig>()
    .BindConfiguration(SyteLineConfig.Seccion)
    .ValidateDataAnnotations()
    .ValidateOnStart();

// ── Base de datos ─────────────────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContextFactory<DBContexto>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

    // Solo activar logs detallados en desarrollo — nunca en producción
    // porque exponen valores de parámetros (contraseñas, datos personales)
    if (builder.Environment.IsDevelopment())
        options.EnableSensitiveDataLogging()
               .EnableDetailedErrors()
               .LogTo(Console.WriteLine, LogLevel.Information);
});

// ── Repositorios ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<IConversionarticuloRepositorio<Conversionarticulo>, ConversionarticuloRepositorio>();
builder.Services.AddScoped<ISroRepositorio, SroRepositorio>();
builder.Services.AddScoped<IValeRecuperoRepositorio, ValeRecuperoRepositorio>();
builder.Services.AddScoped<IValeRecuperoDetalleRepositorio, ValeRecuperoDetalleRepositorio>();
builder.Services.AddScoped<ISyteLineRepositorio, SyteLineRepositorio>();
builder.Services.AddScoped<ITareaOrdenCompraRepositorio, TareaOrdenCompraRepositorio>();

// ── Servicios de aplicación ───────────────────────────────────────────────────
builder.Services.AddScoped<IConversionarticuloServicio, ConversionarticuloServicio>();
builder.Services.AddScoped<ISyteLineHttpClientFactory, SyteLineHttpClient>();
builder.Services.AddScoped<ISyteLineServicio, SyteLineServicio>();
builder.Services.AddScoped<ISyteLinePoServicio, SyteLinePoServicio>();
builder.Services.AddScoped<ITareaOrdenCompraServicio, TareaOrdenCompraServicio>();

// ── Servicios de Vale Recupero ────────────────────────────────────────────────
builder.Services.AddScoped<IValeRecuperoGeneracionServicio, ValeRecuperoGeneracionServicio>();
builder.Services.AddScoped<IValeRecepcionServicio, ValeRecepcionServicio>();
builder.Services.AddScoped<IValeConfirmacionServicio, ValeConfirmacionServicio>();
builder.Services.AddScoped<IValeRecuperoReporteServicio, ValeRecuperoReporteServicio>();
builder.Services.AddScoped<IValeConfirmacionReporteServicio, ValeConfirmacionReporteServicio>();

// SyteLineTokenServicio debe ser Singleton para que la caché del token
// sea compartida entre todas las peticiones. Sus dependencias se resuelven
// a través de IServiceScopeFactory para evitar el anti-patrón
// "Scoped service inside Singleton".
builder.Services.AddSingleton<ISyteLineTokenServicio, SyteLineTokenServicio>();

// ── Infraestructura ───────────────────────────────────────────────────────────
QuestPDF.Settings.License = LicenseType.Community;
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// ── Pipeline HTTP ─────────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Conversionarticulo}/{action=Index}/{id?}");

app.Run();
