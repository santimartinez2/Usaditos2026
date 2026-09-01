using Microsoft.EntityFrameworkCore;
using Usaditos2026.BD.Datos;
using Usaditos2026.BD.Datos.Entity;
using Usaditos2026.Repositorio.Interfaces;

namespace Usaditos2026.Repositorio
{
    public class CarritoRepositorio : ICarritoRepositorio
    {
        private readonly AppDbContext _context;

        public CarritoRepositorio(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Carrito?> ObtenerCarritoActivoConItemsAsync(int clienteId)
        {
            return await _context.Carritos
                .Include(c => c.Items)
                    .ThenInclude(i => i.Producto)
                .Where(c => c.ClienteId == clienteId && c.Estado == "Activo")
                .FirstOrDefaultAsync();
        }

        public async Task<Carrito> CrearCarritoAsync(int clienteId)
        {
            var carrito = new Carrito
            {
                ClienteId = clienteId,
                FechaCreacion = DateTime.UtcNow,
                Estado = "Activo",
                Items = new List<ItemCarrito>()
            };

            _context.Carritos.Add(carrito);
            await _context.SaveChangesAsync();

            return carrito;
        }

        public async Task<Producto?> ObtenerProductoAsync(int productoId)
        {
            return await _context.Productos.FirstOrDefaultAsync(p => p.Id == productoId);
        }

        public async Task<ItemCarrito?> ObtenerItemAsync(int carritoId, int productoId)
        {
            return await _context.ItemsCarrito
                .Include(i => i.Producto)
                .FirstOrDefaultAsync(i => i.CarritoId == carritoId && i.ProductoId == productoId);
        }

        public async Task<ItemCarrito?> ObtenerItemPorIdAsync(int itemId)
        {
            return await _context.ItemsCarrito
                .Include(i => i.Producto)
                .Include(i => i.Carrito)
                .FirstOrDefaultAsync(i => i.Id == itemId);
        }

        public async Task AgregarItemAsync(ItemCarrito item)
        {
            await _context.ItemsCarrito.AddAsync(item);
        }

        public Task EliminarItemAsync(ItemCarrito item)
        {
            _context.ItemsCarrito.Remove(item);
            return Task.CompletedTask;
        }

        public async Task EliminarTodosLosItemsAsync(int carritoId)
        {
            var items = await _context.ItemsCarrito
                .Where(i => i.CarritoId == carritoId)
                .ToListAsync();

            _context.ItemsCarrito.RemoveRange(items);
        }

        public async Task GuardarCambiosAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
