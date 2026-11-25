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

        public async Task<PedidoDto> ConfirmarPedidoAsync(int clienteId, ConfirmarPedidoRequest datos)
        {
            var entrega = ValidarDatosEntregaPago(datos);

            var transaccion = await _pedidoRepositorio.IniciarTransaccionAsync();
            await using (transaccion)
            {
                try
                {
                    var carrito = await _carritoRepositorio.ObtenerCarritoActivoConItemsAsync(clienteId);

                    if (carrito?.Items is null || !carrito.Items.Any())
                        throw new CarritoVacioException("El carrito está vacío. Agregá productos antes de confirmar el pedido.");

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
                        TelefonoContacto = entrega.Telefono,
                        TipoEntrega = entrega.TipoEntrega,
                        DireccionEnvio = entrega.DireccionEnvio,
                        NotaCliente = entrega.NotaCliente,
                        MetodoPago = entrega.MetodoPago,
                        EstadoPago = EstadoPago.Pendiente,
                        FechaPago = null,
                        Items = carrito.Items.Select(item => new ItemPedido
                        {
                            ProductoId = item.ProductoId,
                            NombreProducto = productosPorId[item.ProductoId].Nombre,
                            Cantidad = item.Cantidad,
                            PrecioUnitario = productosPorId[item.ProductoId].Precio
                        }).ToList()
                    };
                    pedido.Total = pedido.Items.Sum(i => i.Cantidad * i.PrecioUnitario);

                    await _pedidoRepositorio.CrearPedidoAsync(pedido);

                    foreach (var item in carrito.Items)
                        productosPorId[item.ProductoId].StockDisponible -= item.Cantidad;

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
                TelefonoContacto = p.TelefonoContacto,
                TipoEntrega = p.TipoEntrega,
                DireccionEnvio = p.DireccionEnvio,
                NotaCliente = p.NotaCliente,
                MetodoPago = p.MetodoPago,
                EstadoPago = p.EstadoPago,
                FechaPago = p.FechaPago,
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
                    ValidarCancelacionDelCliente(pedido);
                    await CancelarPedidoEnTransaccionAsync(pedido, null);
                    await transaccion.CommitAsync();

                    return MapearAPedidoDto(pedido);
                }
                catch (DbUpdateConcurrencyException)
                {
                    await transaccion.RollbackAsync();
                    throw new PedidoModificadoException("El pedido fue modificado, recargá.");
                }
                catch
                {
                    await transaccion.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<PedidoDto> CancelarPedidoComoAdminAsync(int pedidoId, byte[] rowVersion)
        {
            ValidarRowVersion(rowVersion);

            var transaccion = await _pedidoRepositorio.IniciarTransaccionAsync();
            await using (transaccion)
            {
                try
                {
                    var pedido = await _pedidoRepositorio.ObtenerPedidoParaEdicionAsync(pedidoId)
                        ?? throw new KeyNotFoundException("El pedido no existe.");

                    await CancelarPedidoEnTransaccionAsync(pedido, rowVersion);
                    await transaccion.CommitAsync();

                    return MapearAPedidoDto(pedido);
                }
                catch (DbUpdateConcurrencyException)
                {
                    await transaccion.RollbackAsync();
                    throw new PedidoModificadoException("El pedido fue modificado, recargá.");
                }
                catch
                {
                    await transaccion.RollbackAsync();
                    throw;
                }
            }
        }

        private async Task CancelarPedidoEnTransaccionAsync(Pedido pedido, byte[]? rowVersion)
        {
            if (pedido.Estado != PedidoConstantes.EstadoConfirmado &&
                pedido.Estado != PedidoConstantes.EstadoListoParaRetirar)
                throw new EstadoPedidoInvalidoException(
                    $"El pedido no se puede cancelar porque está {pedido.Estado}.");

            foreach (var item in pedido.Items ?? new List<ItemPedido>())
            {
                var producto = await _carritoRepositorio.ObtenerProductoAsync(item.ProductoId);
                if (producto is not null)
                    producto.StockDisponible += item.Cantidad;
            }

            pedido.Estado = PedidoConstantes.EstadoCancelado;

            if (rowVersion is not null)
                await _pedidoRepositorio.EstablecerRowVersionOriginalAsync(pedido, rowVersion);

            await _pedidoRepositorio.GuardarCambiosAsync();
        }

        private static void ValidarCancelacionDelCliente(Pedido pedido)
        {
            if (pedido.Estado != PedidoConstantes.EstadoConfirmado || pedido.EstadoPago != EstadoPago.Pendiente)
                throw new EstadoPedidoInvalidoException(
                    "El pedido ya está pagado o listo para retirar; pedile al local que lo cancele.");
        }

        private static void ValidarRowVersion(byte[] rowVersion)
        {
            if (rowVersion is null || rowVersion.Length == 0)
                throw new ArgumentException("El RowVersion es obligatorio. Obtenelo con GET /api/admin/pedidos/{id}.");
        }

        private static (
            string Telefono,
            TipoEntrega TipoEntrega,
            string? DireccionEnvio,
            MetodoPago MetodoPago,
            string? NotaCliente) ValidarDatosEntregaPago(ConfirmarPedidoRequest datos)
        {
            if (datos is null)
                throw new ArgumentException("Los datos de entrega y pago son obligatorios.");

            var telefono = datos.TelefonoContacto?.Trim() ?? string.Empty;
            if (telefono.Length == 0)
                throw new ArgumentException("El teléfono de contacto es obligatorio.");
            if (telefono.Length > 30)
                throw new ArgumentException("El teléfono no puede superar los 30 caracteres.");
            if (telefono.Any(c => !(char.IsDigit(c) || c == ' ' || c == '+' || c == '-' || c == '(' || c == ')')))
                throw new ArgumentException("El teléfono solo puede tener dígitos, espacios, +, - y paréntesis.");
            if (telefono.Count(char.IsDigit) < 6)
                throw new ArgumentException("El teléfono debe tener al menos 6 dígitos.");

            if (!datos.TipoEntrega.HasValue || !Enum.IsDefined(datos.TipoEntrega.Value))
                throw new ArgumentException("El tipo de entrega es inválido.");
            if (!datos.MetodoPago.HasValue || !Enum.IsDefined(datos.MetodoPago.Value))
                throw new ArgumentException("El método de pago es inválido.");

            var direccion = datos.DireccionEnvio?.Trim();
            if (datos.TipoEntrega.Value == TipoEntrega.Envio && string.IsNullOrEmpty(direccion))
                throw new ArgumentException("La dirección de envío es obligatoria.");
            if (direccion is not null && direccion.Length > 300)
                throw new ArgumentException("La dirección de envío no puede superar los 300 caracteres.");
            if (datos.TipoEntrega.Value == TipoEntrega.Retiro)
                direccion = null;

            var nota = datos.NotaCliente?.Trim();
            if (string.IsNullOrEmpty(nota))
                nota = null;
            if (nota is not null && nota.Length > 500)
                throw new ArgumentException("La nota del cliente no puede superar los 500 caracteres.");

            return (telefono, datos.TipoEntrega.Value, direccion, datos.MetodoPago.Value, nota);
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
                TelefonoContacto = pedido.TelefonoContacto,
                TipoEntrega = pedido.TipoEntrega,
                DireccionEnvio = pedido.DireccionEnvio,
                NotaCliente = pedido.NotaCliente,
                MetodoPago = pedido.MetodoPago,
                EstadoPago = pedido.EstadoPago,
                FechaPago = pedido.FechaPago,
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
