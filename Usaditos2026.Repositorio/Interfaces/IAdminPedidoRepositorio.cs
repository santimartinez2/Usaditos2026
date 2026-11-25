using System.Collections.Generic;
using Usaditos2026.BD.Datos.Entity;
using Usaditos2026.Shared.DTOs;

namespace Usaditos2026.Repositorio.Interfaces
{
    public interface IAdminPedidoRepositorio
    {
        Task<(List<Pedido> Pedidos, int Total)> ListarAsync(PedidoAdminFiltrosDto filtros);
        Task<Pedido?> ObtenerConDetalleAsync(int pedidoId);
        Task<Pedido?> ObtenerParaEdicionAsync(int pedidoId);
        Task EstablecerRowVersionOriginalAsync(Pedido pedido, byte[] rowVersion);
        Task GuardarCambiosAsync();
    }
}
