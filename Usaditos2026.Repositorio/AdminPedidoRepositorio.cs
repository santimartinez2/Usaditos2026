using Microsoft.EntityFrameworkCore;
using Usaditos2026.BD.Datos;
using Usaditos2026.BD.Datos.Entity;
using Usaditos2026.Repositorio.Interfaces;
using Usaditos2026.Shared.DTOs;

namespace Usaditos2026.Repositorio
{
    public class AdminPedidoRepositorio : IAdminPedidoRepositorio
    {
        private readonly AppDbContext _context;

        public AdminPedidoRepositorio(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(List<Pedido> Pedidos, int Total)> ListarAsync(PedidoAdminFiltrosDto filtros)
        {
            var query = _context.Pedidos
                .AsNoTracking()
                .Include(p => p.Cliente)
                .Include(p => p.Items)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtros.Estado))
            {
                var estado = filtros.Estado.Trim();
                query = query.Where(p => p.Estado == estado);
            }

            if (filtros.EstadoPago.HasValue)
                query = query.Where(p => p.EstadoPago == filtros.EstadoPago.Value);

            if (filtros.MetodoPago.HasValue)
                query = query.Where(p => p.MetodoPago == filtros.MetodoPago.Value);

            if (filtros.TipoEntrega.HasValue)
                query = query.Where(p => p.TipoEntrega == filtros.TipoEntrega.Value);

            if (filtros.FechaDesde.HasValue)
                query = query.Where(p => p.Fecha >= filtros.FechaDesde.Value);

            if (filtros.FechaHasta.HasValue)
            {
                var hasta = filtros.FechaHasta.Value.Date.AddDays(1);
                query = query.Where(p => p.Fecha < hasta);
            }

            if (!string.IsNullOrWhiteSpace(filtros.Texto))
            {
                var texto = filtros.Texto.Trim().ToLower();
                query = query.Where(p =>
                    p.Cliente.Nombre.ToLower().Contains(texto) ||
                    p.Cliente.Email.ToLower().Contains(texto));
            }

            var total = await query.CountAsync();

            var pedidos = await query
                .OrderByDescending(p => p.Fecha)
                .ThenByDescending(p => p.Id)
                .Skip((filtros.Page - 1) * filtros.PageSize)
                .Take(filtros.PageSize)
                .ToListAsync();

            return (pedidos, total);
        }

        public async Task<Pedido?> ObtenerConDetalleAsync(int pedidoId)
        {
            return await _context.Pedidos
                .AsNoTracking()
                .Include(p => p.Cliente)
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == pedidoId);
        }

        public async Task<Pedido?> ObtenerParaEdicionAsync(int pedidoId)
        {
            return await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == pedidoId);
        }

        public Task EstablecerRowVersionOriginalAsync(Pedido pedido, byte[] rowVersion)
        {
            _context.Entry(pedido).OriginalValues["RowVersion"] = rowVersion;
            return Task.CompletedTask;
        }

        public async Task GuardarCambiosAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
