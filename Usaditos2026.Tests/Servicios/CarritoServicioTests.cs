using Usaditos2026.Repositorio;
using Usaditos2026.Servicios;
using Usaditos2026.Shared.DTOs;
using Usaditos2026.Shared.Excepciones;
using Usaditos2026.Tests.Helpers;
using Xunit;

namespace Usaditos2026.Tests.Servicios
{
    public class CarritoServicioTests : TestBase
    {
        private readonly CarritoServicio _carritoServicio;

        public CarritoServicioTests()
        {
            var carritoRepo = new CarritoRepositorio(Context);
            _carritoServicio = new CarritoServicio(carritoRepo);
        }

        [Fact]
        public async Task AgregarProducto_ConStockDisponible_LoGuardaEnElCarritoDelCliente()
        {
            // Arrange
            var cliente = await CrearClienteAsync();
            var categoria = await CrearCategoriaAsync();
            var producto = await CrearProductoAsync(categoria.Id, "Remera Dinosaurio", precio: 2000m, stockDisponible: 5);

            var request = new AgregarItemCarritoRequest
            {
                ProductoId = producto.Id,
                Cantidad = 2
            };

            // Act
            var carritoDto = await _carritoServicio.AgregarProductoAsync(cliente.Id, request);

            // Assert
            Assert.NotNull(carritoDto);
            Assert.Equal(cliente.Id, carritoDto.ClienteId);
            Assert.Single(carritoDto.Items);

            var item = carritoDto.Items.First();
            Assert.Equal(producto.Id, item.ProductoId);
            Assert.Equal(2, item.Cantidad);
            Assert.Equal(2000m, item.PrecioUnitario);
            Assert.Equal("Remera Dinosaurio", item.NombreProducto);
        }

        [Fact]
        public async Task AgregarProducto_ConCantidadMayorAlStockDisponible_LanzaStockInsuficienteException()
        {
            // Arrange
            var cliente = await CrearClienteAsync();
            var categoria = await CrearCategoriaAsync();
            var producto = await CrearProductoAsync(categoria.Id, "Jean Clásico", precio: 4500m, stockDisponible: 3);

            var request = new AgregarItemCarritoRequest
            {
                ProductoId = producto.Id,
                Cantidad = 4 // Supera el stock de 3
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<StockInsuficienteException>(() =>
                _carritoServicio.AgregarProductoAsync(cliente.Id, request));

            Assert.Contains("No hay stock suficiente", ex.Message);
        }

        [Fact]
        public async Task AgregarProducto_ConProductoInactivo_LanzaStockInsuficienteException()
        {
            // Arrange
            var cliente = await CrearClienteAsync();
            var categoria = await CrearCategoriaAsync();
            var producto = await CrearProductoAsync(categoria.Id, "Campera Vintage", precio: 6000m, stockDisponible: 5, activo: false);

            var request = new AgregarItemCarritoRequest
            {
                ProductoId = producto.Id,
                Cantidad = 1
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<StockInsuficienteException>(() =>
                _carritoServicio.AgregarProductoAsync(cliente.Id, request));

            Assert.Contains("no tiene stock disponible", ex.Message);
        }

        [Fact]
        public async Task AgregarProducto_ConProductoInexistente_LanzaKeyNotFoundException()
        {
            // Arrange
            var cliente = await CrearClienteAsync();
            var request = new AgregarItemCarritoRequest
            {
                ProductoId = 999999, // Inexistente
                Cantidad = 1
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _carritoServicio.AgregarProductoAsync(cliente.Id, request));

            Assert.Equal("El producto no existe.", ex.Message);
        }

        [Fact]
        public async Task AgregarProducto_ConCantidadQueSumadaAItemExistenteSuperaStock_LanzaStockInsuficienteException()
        {
            // Arrange
            var cliente = await CrearClienteAsync();
            var categoria = await CrearCategoriaAsync();
            var producto = await CrearProductoAsync(categoria.Id, "Buzo Algodón", precio: 3000m, stockDisponible: 3);

            // Agregamos primero 2 unidades (válido)
            await _carritoServicio.AgregarProductoAsync(cliente.Id, new AgregarItemCarritoRequest
            {
                ProductoId = producto.Id,
                Cantidad = 2
            });

            // Intentamos agregar 2 unidades más (2 + 2 = 4 > 3)
            var requestSegundo = new AgregarItemCarritoRequest
            {
                ProductoId = producto.Id,
                Cantidad = 2
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<StockInsuficienteException>(() =>
                _carritoServicio.AgregarProductoAsync(cliente.Id, requestSegundo));

            Assert.Contains("No hay stock suficiente", ex.Message);
        }

        [Fact]
        public async Task ModificarCantidad_SuperandoStockDisponible_LanzaStockInsuficienteException()
        {
            // Arrange
            var cliente = await CrearClienteAsync();
            var categoria = await CrearCategoriaAsync();
            var producto = await CrearProductoAsync(categoria.Id, "Zapatillas", precio: 5000m, stockDisponible: 2);

            var carritoDto = await _carritoServicio.AgregarProductoAsync(cliente.Id, new AgregarItemCarritoRequest
            {
                ProductoId = producto.Id,
                Cantidad = 1
            });
            var itemId = carritoDto.Items.First().Id;

            // Act & Assert
            var ex = await Assert.ThrowsAsync<StockInsuficienteException>(() =>
                _carritoServicio.ModificarCantidadAsync(cliente.Id, itemId, nuevaCantidad: 3));

            Assert.Contains("No hay stock suficiente", ex.Message);
        }

        [Fact]
        public async Task EliminarProducto_ItemExistente_LoEliminaDelCarrito()
        {
            // Arrange
            var cliente = await CrearClienteAsync();
            var categoria = await CrearCategoriaAsync();
            var producto = await CrearProductoAsync(categoria.Id, "Short Verano", precio: 1200m, stockDisponible: 5);

            var carritoDto = await _carritoServicio.AgregarProductoAsync(cliente.Id, new AgregarItemCarritoRequest
            {
                ProductoId = producto.Id,
                Cantidad = 2
            });
            var itemId = carritoDto.Items.First().Id;

            // Act
            var carritoActualizado = await _carritoServicio.EliminarProductoAsync(cliente.Id, itemId);

            // Assert
            Assert.Empty(carritoActualizado.Items);
        }

        [Fact]
        public async Task VaciarCarrito_ConItems_DejaElCarritoVacio()
        {
            // Arrange
            var cliente = await CrearClienteAsync();
            var categoria = await CrearCategoriaAsync();
            var prod1 = await CrearProductoAsync(categoria.Id, "Prod 1", precio: 1000m, stockDisponible: 5);
            var prod2 = await CrearProductoAsync(categoria.Id, "Prod 2", precio: 2000m, stockDisponible: 5);

            await _carritoServicio.AgregarProductoAsync(cliente.Id, new AgregarItemCarritoRequest { ProductoId = prod1.Id, Cantidad = 1 });
            await _carritoServicio.AgregarProductoAsync(cliente.Id, new AgregarItemCarritoRequest { ProductoId = prod2.Id, Cantidad = 2 });

            // Act
            var carritoVacio = await _carritoServicio.VaciarCarritoAsync(cliente.Id);

            // Assert
            Assert.Empty(carritoVacio.Items);
        }
    }
}
