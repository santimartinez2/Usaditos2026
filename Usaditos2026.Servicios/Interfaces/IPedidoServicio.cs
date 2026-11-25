using Usaditos2026.Shared.DTOs;

namespace Usaditos2026.Servicios.Interfaces
{
    public interface IPedidoServicio
    {
        Task<PedidoDto> ConfirmarPedidoAsync(int clienteId);
        Task<List<PedidoResumenDto>> ObtenerMisPedidosAsync(int clienteId);
        Task<PedidoDto> ObtenerPedidoAsync(int clienteId, int pedidoId);
        Task<PedidoDto> CancelarPedidoAsync(int clienteId, int pedidoId);
    }
}
