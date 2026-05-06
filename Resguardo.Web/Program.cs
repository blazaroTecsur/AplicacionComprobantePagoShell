using Resguardo.Web.Middlewares;

var builder = WebApplication.CreateBuilder(args);

var mvcBuilder = builder.Services.AddControllersWithViews();
if (builder.Environment.IsDevelopment())
    mvcBuilder.AddRazorRuntimeCompilation();

ServicesCollectionExtension.AddSettings(builder, builder.Configuration);
ServicesCollectionExtension.AddSeriLog(builder, builder.Configuration);
ServicesCollectionExtension.AddConnectionDataBase(builder.Services, builder.Configuration);
ServicesCollectionExtension.AddServices(builder.Services, builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/resguardo/Error/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UsePathBase("/resguardo");
app.UseStaticFiles();
app.UseRouting();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseStatusCodePagesWithReExecute("/resguardo/Error/Error", "?code={0}");
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
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=SolicitudVisualizar}/{action=Consulta}/{id?}");
app.Run();
