using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Usaditos2026.BD.Datos;
using Usaditos2026.BD.Datos.Entity;
using Usaditos2026.Repositorio.Interfaces;

namespace Usaditos2026.Repositorio
{
    public class PedidoRepositorio : IPedidoRepositorio
    {
        private readonly AppDbContext _context;

        public PedidoRepositorio(AppDbContext context)
        {
            _context = context;
        }

        // El contexto es Scoped: repositorios distintos comparten la misma transacción.
        public async Task<IDbContextTransaction> IniciarTransaccionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        public async Task<Pedido> CrearPedidoAsync(Pedido pedido)
        {
            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

            return pedido;
        }

        public async Task<Pedido?> ObtenerPedidoConItemsAsync(int pedidoId)
        {
            return await _context.Pedidos
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == pedidoId);
        }

        public async Task<Pedido?> ObtenerPedidoParaEdicionAsync(int pedidoId)
        {
            return await _context.Pedidos
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == pedidoId);
        }

        public async Task<List<Pedido>> ObtenerPedidosDeClienteAsync(int clienteId)
        {
            return await _context.Pedidos
                .Include(p => p.Items)
                .Where(p => p.ClienteId == clienteId)
                .OrderByDescending(p => p.Fecha)
                .ThenByDescending(p => p.Id)
                .ToListAsync();
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
