using Usaditos2026.BD.Datos.Entity;
using Usaditos2026.Repositorio.Interfaces;
using Usaditos2026.Servicios.Interfaces;
using Usaditos2026.Shared.DTOs;

namespace Usaditos2026.Servicios
{
    public class CatalogoServicio : ICatalogoServicio
    {
        private readonly ICatalogoRepositorio _catalogoRepositorio;

        public CatalogoServicio(ICatalogoRepositorio catalogoRepositorio)
        {
            _catalogoRepositorio = catalogoRepositorio;
        }

        public async Task<List<ProductoDto>> ObtenerProductosAsync()
        {
            var productos = await _catalogoRepositorio.ListarProductosActivosAsync();

            return productos.Select(MapearAProductoDto).ToList();
        }

        private static ProductoDto MapearAProductoDto(Producto producto)
        {
            return new ProductoDto
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                Precio = producto.Precio,
                StockDisponible = producto.StockDisponible,
                NombreCategoria = producto.Categoria?.Nombre ?? string.Empty
            };
        }
    }
}
