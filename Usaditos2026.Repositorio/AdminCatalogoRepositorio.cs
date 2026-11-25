using Microsoft.EntityFrameworkCore;
using Usaditos2026.BD.Datos;
using Usaditos2026.BD.Datos.Entity;
using Usaditos2026.Repositorio.Interfaces;

namespace Usaditos2026.Repositorio
{
    public class AdminCatalogoRepositorio : IAdminCatalogoRepositorio
    {
        private readonly AppDbContext _context;

        public AdminCatalogoRepositorio(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Categoria>> ListarCategoriasAsync()
        {
            return await _context.Categorias
                .AsNoTracking()
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        public async Task<Categoria?> ObtenerCategoriaPorIdAsync(int id)
        {
            return await _context.Categorias
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Categoria?> ObtenerCategoriaPorNombreAsync(string nombre)
        {
            var nombreBuscado = nombre.ToLower();

            return await _context.Categorias
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Nombre.ToLower() == nombreBuscado);
        }

        public async Task AgregarCategoriaAsync(Categoria categoria)
        {
            await _context.Categorias.AddAsync(categoria);
        }

        public Task EliminarCategoriaAsync(Categoria categoria)
        {
            _context.Categorias.Remove(categoria);
            return Task.CompletedTask;
        }

        public async Task<bool> CategoriaTieneProductosAsync(int categoriaId)
        {
            return await _context.Productos
                .AnyAsync(p => p.CategoriaId == categoriaId);
        }

        public async Task<List<Producto>> ListarProductosAsync()
        {
            return await _context.Productos
                .AsNoTracking()
                .Include(p => p.Categoria)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        public async Task<Producto?> ObtenerProductoAsync(int id)
        {
            return await _context.Productos
                .AsNoTracking()
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Producto?> ObtenerProductoParaEdicionAsync(int id)
        {
            return await _context.Productos
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AgregarProductoAsync(Producto producto)
        {
            await _context.Productos.AddAsync(producto);
        }

        public Task EstablecerRowVersionOriginalAsync(Producto producto, byte[] rowVersion)
        {
            _context.Entry(producto).OriginalValues["RowVersion"] = rowVersion;
            return Task.CompletedTask;
        }

        public async Task GuardarCambiosAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
