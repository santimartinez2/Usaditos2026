using Microsoft.EntityFrameworkCore;
using Usaditos2026.BD.Datos;
using Usaditos2026.BD.Datos.Entity;
using Usaditos2026.Repositorio.Interfaces;

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
    }
}
