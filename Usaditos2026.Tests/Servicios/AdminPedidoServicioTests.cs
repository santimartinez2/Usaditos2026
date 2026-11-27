using Microsoft.Extensions.Options;
using Usaditos2026.BD.Datos.Entity;
using Usaditos2026.Repositorio;
using Usaditos2026.Servicios;
using Usaditos2026.Shared.Constantes;
using Usaditos2026.Shared.DTOs;
using Usaditos2026.Shared.Excepciones;
using Usaditos2026.Tests.Helpers;
using Xunit;

namespace Usaditos2026.Tests.Servicios
{
    public class AdminPedidoServicioTests : TestBase
    {
        private readonly AdminPedidoServicio _adminPedidoServicio;
        private readonly PedidoServicio _pedidoServicio;

        public AdminPedidoServicioTests()
        {
            var adminPedidoRepo = new AdminPedidoRepositorio(Context);
            var pedidoRepo = new PedidoRepositorio(Context);
            var carritoRepo = new CarritoRepositorio(Context);

            _pedidoServicio = new PedidoServicio(pedidoRepo, carritoRepo);

            var tiendaInfo = Options.Create(new TiendaInfoDto
            {
                ZonaHoraria = "America/Argentina/Cordoba"
            });

            _adminPedidoServicio = new AdminPedidoServicio(
                adminPedidoRepo,
                _pedidoServicio,
                tiendaInfo);
        }

        [Fact]
        public async Task CambiarEstado_DeCanceladoAEntregado_LanzaEstadoPedidoInvalidoException()
        {
            // Arrange
            var cliente = await CrearClienteAsync();
            var pedido = new Pedido
            {
                ClienteId = cliente.Id,
                Fecha = DateTime.UtcNow,
                Estado = PedidoConstantes.EstadoCancelado,
                EstadoPago = EstadoPago.Pendiente,
                Total = 1500m,
                RowVersion = new byte[] { 1, 0, 0, 0 }
            };
            Context.Pedidos.Add(pedido);
            await Context.SaveChangesAsync();

            var request = new CambiarEstadoPedidoRequest
            {
                NuevoEstado = PedidoConstantes.EstadoEntregado,
                RowVersion = pedido.RowVersion
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<EstadoPedidoInvalidoException>(() =>
                _adminPedidoServicio.CambiarEstadoAsync(pedido.Id, request));

            Assert.Contains("No se puede pasar de Cancelado a Entregado", ex.Message);
        }

        [Fact]
        public async Task CambiarEstado_AEntregadoSinEstarPagado_LanzaEstadoPedidoInvalidoException()
        {
            // Arrange
            var cliente = await CrearClienteAsync();
            var pedido = new Pedido
            {
                ClienteId = cliente.Id,
                Fecha = DateTime.UtcNow,
                Estado = PedidoConstantes.EstadoListoParaRetirar,
                EstadoPago = EstadoPago.Pendiente, // No pagado
                Total = 3000m,
                RowVersion = new byte[] { 1, 0, 0, 0 }
            };
            Context.Pedidos.Add(pedido);
            await Context.SaveChangesAsync();

            var request = new CambiarEstadoPedidoRequest
            {
                NuevoEstado = PedidoConstantes.EstadoEntregado,
                RowVersion = pedido.RowVersion
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<EstadoPedidoInvalidoException>(() =>
                _adminPedidoServicio.CambiarEstadoAsync(pedido.Id, request));

            Assert.Contains("No se puede entregar un pedido que no está pagado", ex.Message);
        }

        [Fact]
        public async Task CambiarEstado_ConRowVersionDesactualizado_LanzaPedidoModificadoException()
        {
            // Arrange
            var cliente = await CrearClienteAsync();
            var pedido = new Pedido
            {
                ClienteId = cliente.Id,
                Fecha = DateTime.UtcNow,
                Estado = PedidoConstantes.EstadoConfirmado,
                EstadoPago = EstadoPago.Pendiente,
                Total = 2500m,
                RowVersion = new byte[] { 1, 0, 0, 0 }
            };
            Context.Pedidos.Add(pedido);
            await Context.SaveChangesAsync();

            var request = new CambiarEstadoPedidoRequest
            {
                NuevoEstado = PedidoConstantes.EstadoListoParaRetirar,
                RowVersion = new byte[] { 9, 9, 9, 9 } // RowVersion viejo
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<PedidoModificadoException>(() =>
                _adminPedidoServicio.CambiarEstadoAsync(pedido.Id, request));

            Assert.Contains("El pedido fue modificado, recargá", ex.Message);
        }

        [Fact]
        public async Task MarcarPago_ConRowVersionDesactualizado_LanzaPedidoModificadoException()
        {
            // Arrange
            var cliente = await CrearClienteAsync();
            var pedido = new Pedido
            {
                ClienteId = cliente.Id,
                Fecha = DateTime.UtcNow,
                Estado = PedidoConstantes.EstadoConfirmado,
                EstadoPago = EstadoPago.Pendiente,
                Total = 4000m,
                RowVersion = new byte[] { 2, 0, 0, 0 }
            };
            Context.Pedidos.Add(pedido);
            await Context.SaveChangesAsync();

            var request = new MarcarPagoRequest
            {
                EstadoPago = EstadoPago.Pagado,
                RowVersion = new byte[] { 8, 8, 8, 8 } // RowVersion desactualizado
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<PedidoModificadoException>(() =>
                _adminPedidoServicio.MarcarPagoAsync(pedido.Id, request));

            Assert.Contains("El pedido fue modificado, recargá", ex.Message);
        }

        [Fact]
        public async Task CambiarEstado_TransicionValidaConfirmadoAListoParaRetirar_ActualizaEstado()
        {
            // Arrange
            var cliente = await CrearClienteAsync();
            var pedido = new Pedido
            {
                ClienteId = cliente.Id,
                Fecha = DateTime.UtcNow,
                Estado = PedidoConstantes.EstadoConfirmado,
                EstadoPago = EstadoPago.Pendiente,
                Total = 3500m,
                RowVersion = new byte[] { 1, 1, 1, 1 }
            };
            Context.Pedidos.Add(pedido);
            await Context.SaveChangesAsync();

            var request = new CambiarEstadoPedidoRequest
            {
                NuevoEstado = PedidoConstantes.EstadoListoParaRetirar,
                RowVersion = new byte[] { 1, 1, 1, 1 }
            };

            // Act
            var detalle = await _adminPedidoServicio.CambiarEstadoAsync(pedido.Id, request);

            // Assert
            Assert.Equal(PedidoConstantes.EstadoListoParaRetirar, detalle.Estado);
            var pedidoEnDb = await Context.Pedidos.FindAsync(pedido.Id);
            Assert.Equal(PedidoConstantes.EstadoListoParaRetirar, pedidoEnDb!.Estado);
        }
    }
}
