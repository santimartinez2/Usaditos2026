using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Usaditos2026.BD.Datos;
using Usaditos2026.BD.Datos.Entity;
using Usaditos2026.Servicios.Interfaces;
using Usaditos2026.Shared.Constantes;

namespace Usaditos2026.Tests.Helpers
{
    /// <summary>
    /// Base para los tests unitarios y de integración de reglas de negocio.
    /// Crea una base de datos en memoria aislada para cada test.
    /// </summary>
    public abstract class TestBase : IDisposable
    {
        protected AppDbContext Context { get; }

        protected TestBase()
        {
            Context = CrearContexto();
        }

        /// <summary>
        /// Crea una instancia aislada de AppDbContext (UsaditosDbContext) con base de datos en memoria limpia.
        /// Configura un interceptor para simular la generación de RowVersion en INSERT que SQL Server hace por defecto.
        /// </summary>
        public static AppDbContext CrearContexto(string? nombreBase = null)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(nombreBase ?? Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .AddInterceptors(new InMemoryRowVersionInterceptor())
                .Options;

            return new AppDbContext(options);
        }

        /// <summary>
        /// Alias explícito para cumplir con la referencia a UsaditosDbContext pedida en la consigna.
        /// </summary>
        public static AppDbContext CrearUsaditosDbContext(string? nombreBase = null) => CrearContexto(nombreBase);

        protected async Task<Cliente> CrearClienteAsync(
            string nombre = "Cliente Test",
            string email = "cliente@test.com",
            string rol = RolConstantes.Cliente)
        {
            var cliente = new Cliente
            {
                Nombre = nombre,
                Email = email,
                PasswordHash = "hash-dummy",
                Rol = rol
            };
            Context.Clientes.Add(cliente);
            await Context.SaveChangesAsync();
            return cliente;
        }

        protected async Task<Categoria> CrearCategoriaAsync(
            string nombre = "Remeras",
            string descripcion = "Remeras y musculosas")
        {
            var categoria = new Categoria
            {
                Nombre = nombre,
                Descripcion = descripcion
            };
            Context.Categorias.Add(categoria);
            await Context.SaveChangesAsync();
            return categoria;
        }

        protected async Task<Producto> CrearProductoAsync(
            int categoriaId,
            string nombre = "Remera Lisa",
            decimal precio = 1500m,
            int stockDisponible = 10,
            bool activo = true,
            string talle = "Talle único",
            EstadoPrenda estado = EstadoPrenda.MuyBueno,
            byte[]? rowVersion = null)
        {
            var producto = new Producto
            {
                Nombre = nombre,
                Descripcion = "Descripción de prueba",
                Precio = precio,
                StockDisponible = stockDisponible,
                Activo = activo,
                CategoriaId = categoriaId,
                Talle = talle,
                Estado = estado,
                FechaAlta = DateTime.UtcNow,
                RowVersion = rowVersion ?? new byte[] { 1, 0, 0, 0 }
            };
            Context.Productos.Add(producto);
            await Context.SaveChangesAsync();
            return producto;
        }

        public void Dispose()
        {
            Context.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Interceptor para simular en base de datos en memoria la generación automática
    /// de RowVersion (tipo timestamp) que SQL Server realiza nativamente en INSERTs.
    /// </summary>
    public class InMemoryRowVersionInterceptor : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            GenerarRowVersions(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            GenerarRowVersions(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private static void GenerarRowVersions(DbContext? context)
        {
            if (context is null) return;

            foreach (var entry in context.ChangeTracker.Entries())
            {
                if (entry.Entity is Pedido pedido)
                {
                    if (entry.State == EntityState.Added && (pedido.RowVersion == null || pedido.RowVersion.Length == 0))
                    {
                        pedido.RowVersion = Guid.NewGuid().ToByteArray();
                    }
                }
                else if (entry.Entity is Producto producto)
                {
                    if (entry.State == EntityState.Added && (producto.RowVersion == null || producto.RowVersion.Length == 0))
                    {
                        producto.RowVersion = Guid.NewGuid().ToByteArray();
                    }
                }
            }
        }
    }

    /// <summary>
    /// Fake para IAlmacenImagenes para tests sin I/O en disco.
    /// </summary>
    public class AlmacenImagenesFake : IAlmacenImagenes
    {
        public Task<string> GuardarAsync(Stream contenido, string nombreArchivo)
        {
            return Task.FromResult($"/uploads/productos/{nombreArchivo}");
        }

        public Task BorrarAsync(string? rutaRelativa)
        {
            return Task.CompletedTask;
        }
    }
}
