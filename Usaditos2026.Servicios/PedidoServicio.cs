using Microsoft.EntityFrameworkCore;
using Usaditos2026.BD.Datos.Entity;
using Usaditos2026.Repositorio.Interfaces;
using Usaditos2026.Servicios.Interfaces;
using Usaditos2026.Shared.Constantes;
using Usaditos2026.Shared.DTOs;
using Usaditos2026.Shared.Excepciones;

namespace Usaditos2026.Servicios
{
    public class PedidoServicio : IPedidoServicio
    {
        private readonly IPedidoRepositorio _pedidoRepositorio;
        private readonly ICarritoRepositorio _carritoRepositorio;

        public PedidoServicio(IPedidoRepositorio pedidoRepositorio, ICarritoRepositorio carritoRepositorio)
        {
            _pedidoRepositorio = pedidoRepositorio;
            _carritoRepositorio = carritoRepositorio;
        }

        public async Task<PedidoDto> ConfirmarPedidoAsync(int clienteId)
        {
            var transaccion = await _pedidoRepositorio.IniciarTransaccionAsync();
            await using (transaccion)
            {
                try
                {
                    var carrito = await _carritoRepositorio.ObtenerCarritoActivoConItemsAsync(clienteId);

                    if (carrito?.Items is null || !carrito.Items.Any())
                        throw new CarritoVacioException("El carrito está vacío. Agregá productos antes de confirmar el pedido.");

                    // Se releen los productos dentro de la transacción para que RowVersion detecte ventas simultáneas.
                    var productosPorId = new Dictionary<int, Producto>();
                    foreach (var item in carrito.Items)
                    {
                        var producto = await _carritoRepositorio.ObtenerProductoAsync(item.ProductoId)
                            ?? throw new KeyNotFoundException($"El producto con id {item.ProductoId} no existe.");

                        if (!producto.Activo || producto.StockDisponible < item.Cantidad)
                            throw new StockInsuficienteException(
                                $"No hay stock suficiente de '{producto.Nombre}' para confirmar el pedido.");

                        productosPorId[item.ProductoId] = producto;
                    }

                    var pedido = new Pedido
                    {
                        ClienteId = clienteId,
                        Fecha = DateTime.UtcNow,
                        Estado = PedidoConstantes.EstadoConfirmado,
                        Items = carrito.Items.Select(item => new ItemPedido
                        {
                            ProductoId = item.ProductoId,
                            NombreProducto = productosPorId[item.ProductoId].Nombre,
                            Cantidad = item.Cantidad,
                            // Snapshot del precio vigente al confirmar, no del precio guardado en el carrito.
                            PrecioUnitario = productosPorId[item.ProductoId].Precio
                        }).ToList()
                    };
                    pedido.Total = pedido.Items.Sum(i => i.Cantidad * i.PrecioUnitario);

                    await _pedidoRepositorio.CrearPedidoAsync(pedido);

                    foreach (var item in carrito.Items)
                        productosPorId[item.ProductoId].StockDisponible -= item.Cantidad;

                    // El carrito sigue Activo pero vuelve a empezar vacío.
                    await _carritoRepositorio.EliminarTodosLosItemsAsync(carrito.Id);
                    await _pedidoRepositorio.GuardarCambiosAsync();
                    await transaccion.CommitAsync();

                    return MapearAPedidoDto(pedido);
                }
                catch (DbUpdateConcurrencyException)
                {
                    await transaccion.RollbackAsync();
                    throw new StockInsuficienteException(
                        "Otro cliente compró el último stock disponible. Revisá el carrito e intentá de nuevo.");
                }
                catch
                {
                    await transaccion.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<List<PedidoResumenDto>> ObtenerMisPedidosAsync(int clienteId)
        {
            var pedidos = await _pedidoRepositorio.ObtenerPedidosDeClienteAsync(clienteId);

            return pedidos.Select(p => new PedidoResumenDto
            {
                Id = p.Id,
                Fecha = p.Fecha,
                Estado = p.Estado,
                Total = p.Total,
                CantidadItems = p.Items?.Sum(i => i.Cantidad) ?? 0
            }).ToList();
        }

        public async Task<PedidoDto> ObtenerPedidoAsync(int clienteId, int pedidoId)
        {
            var pedido = await ObtenerPedidoDelClienteAsync(clienteId, pedidoId);
            return MapearAPedidoDto(pedido);
        }

        public async Task<PedidoDto> CancelarPedidoAsync(int clienteId, int pedidoId)
        {
            var transaccion = await _pedidoRepositorio.IniciarTransaccionAsync();
            await using (transaccion)
            {
                try
                {
                    var pedido = await ObtenerPedidoDelClienteAsync(clienteId, pedidoId);

                    if (pedido.Estado != PedidoConstantes.EstadoConfirmado)
                        throw new EstadoPedidoInvalidoException(
                            $"El pedido no se puede cancelar porque está {pedido.Estado}.");

                    foreach (var item in pedido.Items)
                    {
                        var producto = await _carritoRepositorio.ObtenerProductoAsync(item.ProductoId);
                        if (producto is not null)
                            producto.StockDisponible += item.Cantidad;
                    }

                    pedido.Estado = PedidoConstantes.EstadoCancelado;
                    await _pedidoRepositorio.GuardarCambiosAsync();
                    await transaccion.CommitAsync();

                    return MapearAPedidoDto(pedido);
                }
                catch (DbUpdateConcurrencyException)
                {
                    await transaccion.RollbackAsync();
                    throw new InvalidOperationException(
                        "No se pudo cancelar el pedido porque otro proceso modificó el pedido o el stock. Intentá de nuevo.");
                }
                catch
                {
                    await transaccion.RollbackAsync();
                    throw;
                }
            }
        }

        private async Task<Pedido> ObtenerPedidoDelClienteAsync(int clienteId, int pedidoId)
        {
            var pedido = await _pedidoRepositorio.ObtenerPedidoConItemsAsync(pedidoId)
                ?? throw new KeyNotFoundException("El pedido no existe.");

            if (pedido.ClienteId != clienteId)
                throw new UnauthorizedAccessException("El pedido no pertenece al cliente.");

            return pedido;
        }

        private static PedidoDto MapearAPedidoDto(Pedido pedido)
        {
            return new PedidoDto
            {
                Id = pedido.Id,
                Fecha = pedido.Fecha,
                Estado = pedido.Estado,
                Items = (pedido.Items ?? new List<ItemPedido>()).Select(i => new ItemPedidoDto
                {
                    ProductoId = i.ProductoId,
                    NombreProducto = i.NombreProducto,
                    Cantidad = i.Cantidad,
                    PrecioUnitario = i.PrecioUnitario
                }).ToList()
            };
        }
    }
}
