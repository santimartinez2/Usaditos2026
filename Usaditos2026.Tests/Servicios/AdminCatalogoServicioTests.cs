using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Usaditos2026.Repositorio;
using Usaditos2026.Servicios;
using Usaditos2026.Servicios.Imagenes;
using Usaditos2026.Shared.Constantes;
using Usaditos2026.Shared.DTOs;
using Usaditos2026.Shared.Excepciones;
using Usaditos2026.Tests.Helpers;
using Xunit;

namespace Usaditos2026.Tests.Servicios
{
    public class AdminCatalogoServicioTests : TestBase
    {
        private readonly AdminCatalogoServicio _adminCatalogoServicio;

        public AdminCatalogoServicioTests()
        {
            var adminCatalogoRepo = new AdminCatalogoRepositorio(Context);
            var opciones = Options.Create(new ImagenesOpciones
            {
                Ruta = "App_Data/uploads/productos",
                TamanoMaximoMb = 8
            });

            _adminCatalogoServicio = new AdminCatalogoServicio(
                adminCatalogoRepo,
                new AlmacenImagenesFake(),
                opciones,
                NullLogger<AdminCatalogoServicio>.Instance);
        }

        [Fact]
        public async Task EliminarCategoria_ConProductosAsociados_LanzaCategoriaConProductosException()
        {
            // Arrange
            var categoria = await CrearCategoriaAsync("Pantalones", "Todo tipo de pantalones");
            await CrearProductoAsync(categoria.Id, "Jean Azul", precio: 3000m, stockDisponible: 5);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<CategoriaConProductosException>(() =>
                _adminCatalogoServicio.EliminarCategoriaAsync(categoria.Id));

            Assert.Contains("porque tiene productos asociados", ex.Message);
        }

        [Fact]
        public async Task EliminarCategoria_SinProductos_LaBorraCorrectamente()
        {
            // Arrange
            var categoria = await CrearCategoriaAsync("Accesorios", "Gorros, bufandas y guantes");

            // Act
            await _adminCatalogoServicio.EliminarCategoriaAsync(categoria.Id);

            // Assert
            var eliminada = await Context.Categorias.FindAsync(categoria.Id);
            Assert.Null(eliminada);
        }

        [Fact]
        public async Task ModificarProducto_ConRowVersionDesactualizado_LanzaProductoModificadoException()
        {
            // Arrange
            var categoria = await CrearCategoriaAsync("Calzado", "Zapatos y zapatillas");
            var versionInicial = new byte[] { 1, 0, 0, 0 };
            var producto = await CrearProductoAsync(categoria.Id, "Zapatillas Lona", precio: 2500m, stockDisponible: 4, rowVersion: versionInicial);

            var request = new ModificarProductoRequest
            {
                Nombre = "Zapatillas Lona Modificadas",
                Descripcion = "Nueva descripción",
                Precio = 2800m,
                StockDisponible = 4,
                CategoriaId = categoria.Id,
                Talle = "Talle único",
                Estado = EstadoPrenda.MuyBueno,
                Observaciones = "Sin detalles",
                RowVersion = new byte[] { 9, 9, 9, 9 } // RowVersion desactualizado/viejo
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ProductoModificadoException>(() =>
                _adminCatalogoServicio.ModificarProductoAsync(producto.Id, request));

            Assert.Contains("El producto fue modificado, recargá", ex.Message);
        }

        [Fact]
        public async Task DesactivarProducto_ConRowVersionDesactualizado_LanzaProductoModificadoException()
        {
            // Arrange
            var categoria = await CrearCategoriaAsync("Abrigos", "Camperas y sacos");
            var versionInicial = new byte[] { 1, 2, 3, 4 };
            var producto = await CrearProductoAsync(categoria.Id, "Campera Abrigo", precio: 7000m, stockDisponible: 2, rowVersion: versionInicial);

            var rowVersionViejo = new byte[] { 8, 8, 8, 8 };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ProductoModificadoException>(() =>
                _adminCatalogoServicio.DesactivarProductoAsync(producto.Id, rowVersionViejo));

            Assert.Contains("El producto fue modificado, recargá", ex.Message);
        }
    }
}
