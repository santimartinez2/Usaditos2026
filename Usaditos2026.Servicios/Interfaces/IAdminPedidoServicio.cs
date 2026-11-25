using Usaditos2026.Shared.DTOs;

namespace Usaditos2026.Servicios.Interfaces
{
    public interface IAdminPedidoServicio
    {
        Task<PaginaDto<PedidoAdminResumenDto>> ObtenerPedidosAsync(PedidoAdminFiltrosDto filtros);
        Task<PedidoAdminDto> ObtenerPedidoAsync(int pedidoId);
        Task<PedidoAdminDto> CambiarEstadoAsync(int pedidoId, CambiarEstadoPedidoRequest request);
        Task<PedidoAdminDto> MarcarPagoAsync(int pedidoId, MarcarPagoRequest request);
    }
}
