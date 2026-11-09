using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Usaditos2026.BD.Datos;
using Usaditos2026.Repositorio;
using Usaditos2026.Repositorio.Interfaces;
using Usaditos2026.Server.Client.Pages;
using Usaditos2026.Server.Components;
using Usaditos2026.Server.Data;
using Usaditos2026.Servicios;
using Usaditos2026.Servicios.Interfaces;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("ConnSqlServer")
    ?? throw new InvalidOperationException("No existe la conexión con la base de datos.");

#region Servicios

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddScoped<ICarritoRepositorio, CarritoRepositorio>();
builder.Services.AddScoped<ICarritoServicio, CarritoServicio>();
builder.Services.AddScoped<ICatalogoRepositorio, CatalogoRepositorio>();
builder.Services.AddScoped<ICatalogoServicio, CatalogoServicio>();
builder.Services.AddScoped<IClienteRepositorio, ClienteRepositorio>();
builder.Services.AddScoped<IAuthServicio, AuthServicio>();

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

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

#endregion

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await SeedData.InicializarAsync(db);
}

#region Middleware

if (app.Environment.IsDevelopment())
{
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

