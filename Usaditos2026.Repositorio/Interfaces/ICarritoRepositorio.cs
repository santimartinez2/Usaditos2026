using Usaditos2026.BD.Datos.Entity;

namespace Usaditos2026.Repositorio.Interfaces
{
    public interface ICarritoRepositorio
    {
        Task<Carrito?> ObtenerCarritoActivoConItemsAsync(int clienteId);
        Task<Carrito> CrearCarritoAsync(int clienteId);
        Task<Producto?> ObtenerProductoAsync(int productoId);
        Task<ItemCarrito?> ObtenerItemAsync(int carritoId, int productoId);
        Task<ItemCarrito?> ObtenerItemPorIdAsync(int itemId);
        Task AgregarItemAsync(ItemCarrito item);
        Task EliminarItemAsync(ItemCarrito item);
        Task EliminarTodosLosItemsAsync(int carritoId);
        Task GuardarCambiosAsync();
    }
}
