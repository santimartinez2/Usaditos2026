using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Storage;
using Usaditos2026.BD.Datos.Entity;

namespace Usaditos2026.Repositorio.Interfaces
{
    public interface IPedidoRepositorio
    {
        Task<IDbContextTransaction> IniciarTransaccionAsync();
        Task<Pedido> CrearPedidoAsync(Pedido pedido);
        Task<Pedido?> ObtenerPedidoConItemsAsync(int pedidoId);
        Task<Pedido?> ObtenerPedidoParaEdicionAsync(int pedidoId);
        Task<List<Pedido>> ObtenerPedidosDeClienteAsync(int clienteId);
        Task EstablecerRowVersionOriginalAsync(Pedido pedido, byte[] rowVersion);
        Task GuardarCambiosAsync();
    }
}
