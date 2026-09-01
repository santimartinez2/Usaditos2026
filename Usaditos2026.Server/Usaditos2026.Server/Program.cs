using Microsoft.EntityFrameworkCore;
using Usaditos2026.BD.Datos;
using Usaditos2026.Repositorio;
using Usaditos2026.Repositorio.Interfaces;
using Usaditos2026.Server.Client.Pages;
using Usaditos2026.Server.Components;
using Usaditos2026.Servicios;
using Usaditos2026.Servicios.Interfaces;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("ConnSqlServer")
    ?? throw new InvalidOperationException("No existe la conexión con la base de datos.");

#region Servicios

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

// Repositorio y Servicios del caso de uso "Agregar y eliminar producto del carrito".
builder.Services.AddScoped<ICarritoRepositorio, CarritoRepositorio>();
builder.Services.AddScoped<ICarritoServicio, CarritoServicio>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

#endregion

var app = builder.Build();

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
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Usaditos2026.Server.Client._Imports).Assembly);

#endregion

app.Run();

