using Microsoft.EntityFrameworkCore;
using Usaditos2026.BD.Datos;
using Usaditos2026.BD.Datos.Entity;
using Usaditos2026.Repositorio.Interfaces;
using Usaditos2026.Shared.Constantes;
using Usaditos2026.Shared.DTOs;

namespace Usaditos2026.Repositorio
{
    public class CatalogoRepositorio : ICatalogoRepositorio
    {
        private readonly AppDbContext _context;

        public CatalogoRepositorio(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Producto>> ListarProductosActivosAsync()
        {
            return await _context.Productos
                .AsNoTracking()
                .Include(p => p.Categoria)
                .Where(p => p.Activo)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        public async Task<List<(int Id, string? ImagenUrl)>> ObtenerImagenesAsync(IReadOnlyCollection<int> productoIds)
        {
            var datos = await _context.Productos
                .AsNoTracking()
                .Where(p => productoIds.Contains(p.Id))
                .Select(p => new { p.Id, p.ImagenUrl })
                .ToListAsync();

            return datos.Select(x => (x.Id, x.ImagenUrl)).ToList();
        }

        public async Task<Producto?> ObtenerProductoActivoAsync(int id)
        {
            return await _context.Productos
                .AsNoTracking()
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.Activo && p.Id == id);
        }

        public async Task<List<Producto>> ListarRelacionadosAsync(int id, int categoriaId, int cantidad)
        {
            return await _context.Productos
                .AsNoTracking()
                .Include(p => p.Categoria)
                .Where(p => p.Activo && p.StockDisponible > 0 && p.CategoriaId == categoriaId && p.Id != id)
                .OrderByDescending(p => p.FechaAlta)
                .ThenByDescending(p => p.Id)
                .Take(cantidad)
                .ToListAsync();
        }

        public async Task<(List<Producto> Productos, int Total)> BuscarAsync(CatalogoFiltrosDto filtros)
        {
            IQueryable<Producto> query = _context.Productos
                .AsNoTracking()
                .Include(p => p.Categoria)
                .Where(p => p.Activo);

            if (filtros.CategoriaId.HasValue)
                query = query.Where(p => p.CategoriaId == filtros.CategoriaId.Value);

            if (!string.IsNullOrWhiteSpace(filtros.Talle))
            {
                var talle = filtros.Talle.Trim();
                query = query.Where(p => p.Talle == talle);
            }

            if (filtros.Estado.HasValue)
                query = query.Where(p => p.Estado == filtros.Estado.Value);

            if (!string.IsNullOrWhiteSpace(filtros.Texto))
            {
                var texto = filtros.Texto.Trim().ToLower();
                query = query.Where(p =>
                    p.Nombre.ToLower().Contains(texto) ||
                    p.Descripcion.ToLower().Contains(texto));
            }

            if (filtros.SoloDisponibles)
                query = query.Where(p => p.StockDisponible > 0);

            var total = await query.CountAsync();

            query = filtros.Orden switch
            {
                OrdenCatalogo.PrecioMenor => query
                    .OrderBy(p => p.Precio)
                    .ThenByDescending(p => p.FechaAlta)
                    .ThenByDescending(p => p.Id),
                OrdenCatalogo.PrecioMayor => query
                    .OrderByDescending(p => p.Precio)
                    .ThenByDescending(p => p.FechaAlta)
                    .ThenByDescending(p => p.Id),
                _ => query
                    .OrderByDescending(p => p.FechaAlta)
                    .ThenByDescending(p => p.Id)
            };

            var productos = await query
                .Skip((filtros.Page - 1) * filtros.PageSize)
                .Take(filtros.PageSize)
                .ToListAsync();

            return (productos, total);
        }

        public async Task<List<(Categoria Categoria, int Cantidad)>> ListarCategoriasConCantidadAsync()
        {
            var datos = await _context.Categorias
                .AsNoTracking()
                .GroupJoin(
                    _context.Productos.Where(p => p.Activo),
                    categoria => categoria.Id,
                    producto => producto.CategoriaId,
                    (categoria, productos) => new
                    {
                        Categoria = categoria,
                        Cantidad = productos.Count()
                    })
                .OrderBy(x => x.Categoria.Nombre)
                .ToListAsync();

            return datos.Select(x => (x.Categoria, x.Cantidad)).ToList();
        }

        public async Task<List<(string Talle, int Cantidad)>> ListarTallesConCantidadAsync()
        {
            var datos = await _context.Productos
                .AsNoTracking()
                .Where(p => p.Activo)
                .GroupBy(p => p.Talle)
                .Select(grupo => new
                {
                    Talle = grupo.Key,
                    Cantidad = grupo.Count()
                })
                .ToListAsync();

            return datos.Select(x => (x.Talle, x.Cantidad)).ToList();
        }

        public async Task<List<(EstadoPrenda Estado, int Cantidad)>> ListarEstadosConCantidadAsync()
        {
            var datos = await _context.Productos
                .AsNoTracking()
                .Where(p => p.Activo)
                .GroupBy(p => p.Estado)
                .Select(grupo => new
                {
                    Estado = grupo.Key,
                    Cantidad = grupo.Count()
                })
                .ToListAsync();

            return datos.Select(x => (x.Estado, x.Cantidad)).ToList();
        }
    }
}
