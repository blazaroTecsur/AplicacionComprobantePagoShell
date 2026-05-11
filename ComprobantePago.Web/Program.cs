using ComprobantePago.Infrastructure.Persistence;
using ComprobantePago.Web.Middlewares;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.AddSeriLog();
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddServices(builder.Configuration);

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
    Log.Warning(ex, "No se pudo ejecutar la migración de columnas al iniciar.");
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
    if (!(context.User?.Identity?.IsAuthenticated ?? false))
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
    pattern: "{controller=Comprobante}/{action=Index}/{id?}");

app.Run();
