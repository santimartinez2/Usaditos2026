using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Usaditos2026.BD.Datos;
using Usaditos2026.Repositorio;
using Usaditos2026.Repositorio.Interfaces;
using Usaditos2026.Server.Client.Pages;
using Usaditos2026.Server.Components;
using Usaditos2026.Server.Data;
using Usaditos2026.Server.Middleware;
using Usaditos2026.Server.Storage;
using Usaditos2026.Servicios;
using Usaditos2026.Servicios.Interfaces;
using Usaditos2026.Shared.Constantes;
using Usaditos2026.Shared.DTOs;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("ConnSqlServer")
    ?? throw new InvalidOperationException("No existe la conexión con la base de datos.");

#region Servicios

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddScoped<IAdminCatalogoRepositorio, AdminCatalogoRepositorio>();
builder.Services.AddScoped<IAdminCatalogoServicio, AdminCatalogoServicio>();
builder.Services.AddScoped<IAdminPedidoRepositorio, AdminPedidoRepositorio>();
builder.Services.AddScoped<IAdminPedidoServicio, AdminPedidoServicio>();
builder.Services.AddScoped<ICarritoRepositorio, CarritoRepositorio>();
builder.Services.AddScoped<ICarritoServicio, CarritoServicio>();
builder.Services.AddScoped<ICatalogoRepositorio, CatalogoRepositorio>();
builder.Services.AddScoped<ICatalogoServicio, CatalogoServicio>();
builder.Services.AddScoped<IClienteRepositorio, ClienteRepositorio>();
builder.Services.AddScoped<IPedidoRepositorio, PedidoRepositorio>();
builder.Services.AddScoped<IAuthServicio, AuthServicio>();
builder.Services.AddScoped<IPedidoServicio, PedidoServicio>();
builder.Services.AddScoped<ITiendaServicio, TiendaServicio>();
builder.Services.AddScoped<IAlmacenImagenes, AlmacenImagenes>();
builder.Services.Configure<TiendaInfoDto>(builder.Configuration.GetSection("Tienda"));
builder.Services.Configure<ImagenesOpciones>(builder.Configuration.GetSection("Imagenes"));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login";
        options.Cookie.Name = "Usaditos2026.Auth";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;

        options.Events.OnRedirectToLogin = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }

            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };

        options.Events.OnRedirectToAccessDenied = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            }

            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AutorizacionConstantes.PoliticaSoloAdmin,
        policy => policy.RequireRole(RolConstantes.Admin));
});
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(allowIntegerValues: false)));
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ManejadorGlobalExcepciones>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

#endregion

var app = builder.Build();

// Las imágenes se guardan fuera de wwwroot y se sirven desde una ruta pública inmutable.
var opcionesImagenes = builder.Configuration.GetSection("Imagenes").Get<ImagenesOpciones>() ?? new ImagenesOpciones();
var rutaImagenes = Path.IsPathRooted(opcionesImagenes.Ruta)
    ? opcionesImagenes.Ruta
    : Path.Combine(builder.Environment.ContentRootPath, opcionesImagenes.Ruta.Replace('/', Path.DirectorySeparatorChar));
Directory.CreateDirectory(rutaImagenes);

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await SeedData.InicializarAsync(db);
    await SeedData.CrearAdminInicialAsync(db, builder.Configuration, app.Logger);
}

#region Middleware

if (app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
    app.UseWebAssemblyDebugging();
    app.UseSwagger();
    app.UseSwaggerUI(); // Disponible en /swagger
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
   
    app.UseHsts();
}
app.UseWhen(
    context => !context.Request.Path.StartsWithSegments("/api"),
    branch =>
    {
        branch.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
    });

app.UseHttpsRedirection();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(rutaImagenes),
    RequestPath = ImagenesConstantes.RutaPublica,
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers["Cache-Control"] = "public, max-age=31536000, immutable";
        ctx.Context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    }
});

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Usaditos2026.Server.Client._Imports).Assembly);

#endregion

app.Run();

