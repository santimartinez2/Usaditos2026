using Usaditos2026.BD.Datos.Entity;
using Usaditos2026.Repositorio.Interfaces;
using Usaditos2026.Servicios.Interfaces;
using Usaditos2026.Shared.DTOs;
using Usaditos2026.Shared.Excepciones;

namespace Usaditos2026.Servicios
{
    public class CarritoServicio : ICarritoServicio
    {
        private readonly ICarritoRepositorio _carritoRepositorio;

        public CarritoServicio(ICarritoRepositorio carritoRepositorio)
        {
            _carritoRepositorio = carritoRepositorio;
        }

        public async Task<CarritoDto> ObtenerCarritoAsync(int clienteId)
        {
            var carrito = await ObtenerOCrearCarritoActivoAsync(clienteId);
            return MapearACarritoDto(carrito);
        }

        public async Task<CarritoDto> AgregarProductoAsync(AgregarItemCarritoRequest request)
        {
            if (request.Cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a cero.");

            // 3. El sistema verifica que el producto tenga stock disponible.
            var producto = await _carritoRepositorio.ObtenerProductoAsync(request.ProductoId)
                ?? throw new KeyNotFoundException("El producto no existe.");

            if (!producto.Activo || producto.StockDisponible < request.Cantidad)
                // 3a. El producto no tiene stock disponible: se notifica al cliente.
                throw new StockInsuficienteException(
                    $"El producto '{producto.Nombre}' no tiene stock disponible.");

            var carrito = await ObtenerOCrearCarritoActivoAsync(request.ClienteId);

            var itemExistente = await _carritoRepositorio.ObtenerItemAsync(carrito.Id, request.ProductoId);

            if (itemExistente is not null)
            {
                // El producto ya estaba en el carrito: se suma la cantidad (5a).
                var cantidadTotal = itemExistente.Cantidad + request.Cantidad;
                if (producto.StockDisponible < cantidadTotal)
                    throw new StockInsuficienteException(
                        $"No hay stock suficiente de '{producto.Nombre}' para esa cantidad.");

                itemExistente.Cantidad = cantidadTotal;
            }
            else
            {
                // 4. El sistema agrega el producto al carrito y actualiza el total.
                var nuevoItem = new ItemCarrito
                {
                    CarritoId = carrito.Id,
                    ProductoId = producto.Id,
                    Cantidad = request.Cantidad,
                    PrecioUnitario = producto.Precio
                };
                await _carritoRepositorio.AgregarItemAsync(nuevoItem);
            }

            await _carritoRepositorio.GuardarCambiosAsync();

            var carritoActualizado = await _carritoRepositorio.ObtenerCarritoActivoConItemsAsync(request.ClienteId);
            return MapearACarritoDto(carritoActualizado!);
        }

        public async Task<CarritoDto> EliminarProductoAsync(int clienteId, int itemId)
        {
            var item = await _carritoRepositorio.ObtenerItemPorIdAsync(itemId)
                ?? throw new KeyNotFoundException("El producto no está en el carrito.");

            if (item.Carrito.ClienteId != clienteId)
                throw new UnauthorizedAccessException("El item no pertenece al carrito del cliente.");

            // 7-8. El sistema elimina el producto y recalcula el total.
            await _carritoRepositorio.EliminarItemAsync(item);
            await _carritoRepositorio.GuardarCambiosAsync();

            var carrito = await ObtenerOCrearCarritoActivoAsync(clienteId);
            return MapearACarritoDto(carrito);
        }

        public async Task<CarritoDto> ModificarCantidadAsync(int clienteId, int itemId, int nuevaCantidad)
        {
            if (nuevaCantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a cero. Para quitar el producto, elimínalo del carrito.");

            var item = await _carritoRepositorio.ObtenerItemPorIdAsync(itemId)
                ?? throw new KeyNotFoundException("El producto no está en el carrito.");

            if (item.Carrito.ClienteId != clienteId)
                throw new UnauthorizedAccessException("El item no pertenece al carrito del cliente.");

            if (item.Producto.StockDisponible < nuevaCantidad)
                throw new StockInsuficienteException(
                    $"No hay stock suficiente de '{item.Producto.Nombre}' para esa cantidad.");

            // 5a. El sistema actualiza la cantidad y recalcula el total.
            item.Cantidad = nuevaCantidad;
            await _carritoRepositorio.GuardarCambiosAsync();

            var carrito = await ObtenerOCrearCarritoActivoAsync(clienteId);
            return MapearACarritoDto(carrito);
        }

        public async Task<CarritoDto> VaciarCarritoAsync(int clienteId)
        {
            var carrito = await ObtenerOCrearCarritoActivoAsync(clienteId);

            // 7a. El cliente decide vaciar todo el carrito.
            await _carritoRepositorio.EliminarTodosLosItemsAsync(carrito.Id);
            await _carritoRepositorio.GuardarCambiosAsync();

            var carritoActualizado = await ObtenerOCrearCarritoActivoAsync(clienteId);
            return MapearACarritoDto(carritoActualizado);
        }

        private async Task<Carrito> ObtenerOCrearCarritoActivoAsync(int clienteId)
        {
            var carrito = await _carritoRepositorio.ObtenerCarritoActivoConItemsAsync(clienteId);
            return carrito ?? await _carritoRepositorio.CrearCarritoAsync(clienteId);
        }

        private static CarritoDto MapearACarritoDto(Carrito carrito)
        {
            return new CarritoDto
            {
                Id = carrito.Id,
                ClienteId = carrito.ClienteId,
                Estado = carrito.Estado,
                FechaCreacion = carrito.FechaCreacion,
                // 6a. Si no hay items, se devuelve la lista vacía (el front muestra "carrito vacío").
                Items = (carrito.Items ?? new List<ItemCarrito>()).Select(i => new ItemCarritoDto
                {
                    Id = i.Id,
                    ProductoId = i.ProductoId,
                    NombreProducto = i.Producto?.Nombre ?? string.Empty,
                    Cantidad = i.Cantidad,
                    PrecioUnitario = i.PrecioUnitario
                }).ToList()
            };
        }
    }
}
