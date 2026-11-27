using Microsoft.EntityFrameworkCore;
using Usaditos2026.Repositorio;
using Usaditos2026.Servicios;
using Usaditos2026.Shared.Constantes;
using Usaditos2026.Shared.DTOs;
using Usaditos2026.Shared.Excepciones;
using Usaditos2026.Tests.Helpers;
using Xunit;

namespace Usaditos2026.Tests.Servicios
{
    public class PedidoServicioTests : TestBase
    {
        private readonly PedidoServicio _pedidoServicio;
        private readonly CarritoServicio _carritoServicio;
        private readonly PedidoRepositorio _pedidoRepo;
        private readonly CarritoRepositorio _carritoRepo;

        public PedidoServicioTests()
        {
            _pedidoRepo = new PedidoRepositorio(Context);
            _carritoRepo = new CarritoRepositorio(Context);
            _pedidoServicio = new PedidoServicio(_pedidoRepo, _carritoRepo);
            _carritoServicio = new CarritoServicio(_carritoRepo);
        }

        [Fact]
        public async Task CrearPedido_DesdeCarrito_DescuentaStockVaciaCarritoYCalculaTotal()
        {
            // Arrange
            var cliente = await CrearClienteAsync();
            var categoria = await CrearCategoriaAsync();
            var prod1 = await CrearProductoAsync(categoria.Id, "Remera Dinosaurio", precio: 1500m, stockDisponible: 10);
            var prod2 = await CrearProductoAsync(categoria.Id, "Pantalón Jean", precio: 2500m, stockDisponible: 5);

            // Cargamos el carrito: 2 remeras (3000) + 1 pantalón (2500) = 5500
            await _carritoServicio.AgregarProductoAsync(cliente.Id, new AgregarItemCarritoRequest
            {
                ProductoId = prod1.Id,
                Cantidad = 2
            });
            await _carritoServicio.AgregarProductoAsync(cliente.Id, new AgregarItemCarritoRequest
            {
                ProductoId = prod2.Id,
                Cantidad = 1
            });

            var checkoutRequest = new ConfirmarPedidoRequest
            {
                TelefonoContacto = "3511234567",
                TipoEntrega = TipoEntrega.Retiro,
                MetodoPago = MetodoPago.Efectivo,
                NotaCliente = "Lo busco por la tarde"
            };

            // Act
            var pedidoDto = await _pedidoServicio.ConfirmarPedidoAsync(cliente.Id, checkoutRequest);

            // Assert
            Assert.NotNull(pedidoDto);
            Assert.Equal(PedidoConstantes.EstadoConfirmado, pedidoDto.Estado);
            Assert.Equal(EstadoPago.Pendiente, pedidoDto.EstadoPago);
            Assert.Equal(5500m, pedidoDto.Total);
            Assert.Equal(2, pedidoDto.Items.Count);

            // Verificar que se descontó el stock de la base de datos
            var prod1Actualizado = await Context.Productos.FindAsync(prod1.Id);
            var prod2Actualizado = await Context.Productos.FindAsync(prod2.Id);
            Assert.Equal(8, prod1Actualizado!.StockDisponible);
            Assert.Equal(4, prod2Actualizado!.StockDisponible);

            // Verificar que el carrito quedó vacío
            var carritoActualizado = await _carritoRepo.ObtenerCarritoActivoConItemsAsync(cliente.Id);
            Assert.NotNull(carritoActualizado);
            Assert.Empty(carritoActualizado.Items);
        }

        [Fact]
        public async Task ConfirmarPedido_ConCarritoVacio_LanzaCarritoVacioException()
        {
            // Arrange
            var cliente = await CrearClienteAsync();

            var checkoutRequest = new ConfirmarPedidoRequest
            {
                TelefonoContacto = "3511234567",
                TipoEntrega = TipoEntrega.Retiro,
                MetodoPago = MetodoPago.Efectivo
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<CarritoVacioException>(() =>
                _pedidoServicio.ConfirmarPedidoAsync(cliente.Id, checkoutRequest));

            Assert.Contains("El carrito está vacío", ex.Message);
        }

        [Fact]
        public async Task CancelarPedido_DevuelveStockDisponibleDeCadaProducto()
        {
            // Arrange
            var cliente = await CrearClienteAsync();
            var categoria = await CrearCategoriaAsync();
            var prod = await CrearProductoAsync(categoria.Id, "Buzo Friza", precio: 4000m, stockDisponible: 5);

            await _carritoServicio.AgregarProductoAsync(cliente.Id, new AgregarItemCarritoRequest
            {
                ProductoId = prod.Id,
                Cantidad = 3
            });

            var checkoutRequest = new ConfirmarPedidoRequest
            {
                TelefonoContacto = "3511234567",
                TipoEntrega = TipoEntrega.Retiro,
                MetodoPago = MetodoPago.Efectivo
            };

            var pedidoDto = await _pedidoServicio.ConfirmarPedidoAsync(cliente.Id, checkoutRequest);

            // Verificar que el stock bajó a 2
            var prodPostCheckout = await Context.Productos.FindAsync(prod.Id);
            Assert.Equal(2, prodPostCheckout!.StockDisponible);

            // Act - Cliente cancela el pedido
            var pedidoCancelado = await _pedidoServicio.CancelarPedidoAsync(cliente.Id, pedidoDto.Id);

            // Assert
            Assert.Equal(PedidoConstantes.EstadoCancelado, pedidoCancelado.Estado);

            // El stock debe haber vuelto a 5
            var prodPostCancelacion = await Context.Productos.FindAsync(prod.Id);
            Assert.Equal(5, prodPostCancelacion!.StockDisponible);
        }

        [Fact]
        public async Task CancelarPedido_CuandoYaEstaCancelado_LanzaEstadoPedidoInvalidoException()
        {
            // Arrange
            var cliente = await CrearClienteAsync();
            var categoria = await CrearCategoriaAsync();
            var prod = await CrearProductoAsync(categoria.Id, "Campera", precio: 8000m, stockDisponible: 3);

            await _carritoServicio.AgregarProductoAsync(cliente.Id, new AgregarItemCarritoRequest
            {
                ProductoId = prod.Id,
                Cantidad = 1
            });

            var checkoutRequest = new ConfirmarPedidoRequest
            {
                TelefonoContacto = "3511234567",
                TipoEntrega = TipoEntrega.Retiro,
                MetodoPago = MetodoPago.Efectivo
            };

            var pedidoDto = await _pedidoServicio.ConfirmarPedidoAsync(cliente.Id, checkoutRequest);

            // Cancelamos una primera vez
            await _pedidoServicio.CancelarPedidoAsync(cliente.Id, pedidoDto.Id);

            // Act & Assert - Intentar cancelar nuevamente un pedido que ya está Cancelado
            var ex = await Assert.ThrowsAsync<EstadoPedidoInvalidoException>(() =>
                _pedidoServicio.CancelarPedidoAsync(cliente.Id, pedidoDto.Id));

            Assert.Contains("El pedido ya está pagado o listo para retirar", ex.Message);
        }

        [Fact]
        public async Task CancelarPedidoComoAdmin_CuandoElPedidoYaEstaEntregado_LanzaEstadoPedidoInvalidoException()
        {
            // Arrange
            var cliente = await CrearClienteAsync();
            var categoria = await CrearCategoriaAsync();
            var prod = await CrearProductoAsync(categoria.Id, "Gorro", precio: 1000m, stockDisponible: 5);

            await _carritoServicio.AgregarProductoAsync(cliente.Id, new AgregarItemCarritoRequest
            {
                ProductoId = prod.Id,
                Cantidad = 1
            });

            var checkoutRequest = new ConfirmarPedidoRequest
            {
                TelefonoContacto = "3511234567",
                TipoEntrega = TipoEntrega.Retiro,
                MetodoPago = MetodoPago.Efectivo
            };

            var pedidoDto = await _pedidoServicio.ConfirmarPedidoAsync(cliente.Id, checkoutRequest);

            // Pasamos a Entregado a nivel de entidad
            var pedido = await Context.Pedidos.FindAsync(pedidoDto.Id);
            pedido!.Estado = PedidoConstantes.EstadoEntregado;
            pedido.EstadoPago = EstadoPago.Pagado;
            await Context.SaveChangesAsync();

            // Act & Assert - Admin intenta cancelar un pedido ya Entregado
            var ex = await Assert.ThrowsAsync<EstadoPedidoInvalidoException>(() =>
                _pedidoServicio.CancelarPedidoComoAdminAsync(pedidoDto.Id, pedido.RowVersion ?? new byte[] { 1, 0, 0, 0 }));

            Assert.Contains("El pedido no se puede cancelar porque está Entregado", ex.Message);
        }
    }
}
