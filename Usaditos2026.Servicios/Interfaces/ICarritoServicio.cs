using Usaditos2026.Shared.DTOs;

namespace Usaditos2026.Servicios.Interfaces
{
    public interface ICarritoServicio
    {
        Task<CarritoDto> ObtenerCarritoAsync(int clienteId);
        Task<CarritoDto> AgregarProductoAsync(AgregarItemCarritoRequest request);
        Task<CarritoDto> EliminarProductoAsync(int clienteId, int itemId);
        Task<CarritoDto> ModificarCantidadAsync(int clienteId, int itemId, int nuevaCantidad);
        Task<CarritoDto> VaciarCarritoAsync(int clienteId);
    }
}
