using System;
using Usaditos2026.BD.Datos.Entity;
using Usaditos2026.Repositorio.Interfaces;
using Usaditos2026.Servicios.Interfaces;
using Usaditos2026.Shared.Constantes;
using Usaditos2026.Shared.DTOs;
using Usaditos2026.Shared.Utilidades;

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

        public async Task<Dictionary<int, string?>> ObtenerImagenesAsync(IEnumerable<int> productoIds)
        {
            if (productoIds is null)
                return new Dictionary<int, string?>();

            var ids = productoIds
                .Where(id => id > 0)
                .Distinct()
                .ToArray();

            if (ids.Length == 0)
                return new Dictionary<int, string?>();

            if (ids.Length > CatalogoConstantes.MaximoIdsImagenes)
                throw new ArgumentException($"No se pueden consultar más de {CatalogoConstantes.MaximoIdsImagenes} productos a la vez.");

            var imagenes = await _catalogoRepositorio.ObtenerImagenesAsync(ids);

            return imagenes.ToDictionary(x => x.Id, x => x.ImagenUrl);
        }

        public async Task<ProductoDto> ObtenerProductoAsync(int id)
        {
            var producto = await _catalogoRepositorio.ObtenerProductoActivoAsync(id)
                ?? throw new KeyNotFoundException("El producto no existe o no está activo.");

            return MapearAProductoDto(producto);
        }

        public async Task<List<ProductoDto>> ObtenerRelacionadosAsync(
            int id, int cantidad = CatalogoConstantes.RelacionadosPorDefecto)
        {
            if (cantidad < CatalogoConstantes.RelacionadosMinimos ||
                cantidad > CatalogoConstantes.RelacionadosMaximos)
                throw new ArgumentException(
                    $"La cantidad debe estar entre {CatalogoConstantes.RelacionadosMinimos} y {CatalogoConstantes.RelacionadosMaximos}.");

            var producto = await _catalogoRepositorio.ObtenerProductoActivoAsync(id)
                ?? throw new KeyNotFoundException("El producto no existe o no está activo.");

            var relacionados = await _catalogoRepositorio.ListarRelacionadosAsync(
                id, producto.CategoriaId, cantidad);

            return relacionados.Select(MapearAProductoDto).ToList();
        }

        public async Task<PaginaDto<ProductoDto>> BuscarProductosAsync(CatalogoFiltrosDto filtros)
        {
            var filtrosValidos = NormalizarFiltros(filtros);
            var resultado = await _catalogoRepositorio.BuscarAsync(filtrosValidos);

            return new PaginaDto<ProductoDto>
            {
                Items = resultado.Productos.Select(MapearAProductoDto).ToList(),
                Total = resultado.Total,
                Page = filtrosValidos.Page,
                PageSize = filtrosValidos.PageSize
            };
        }

        public async Task<OpcionesFiltroCatalogoDto> ObtenerOpcionesFiltroAsync()
        {
            var categorias = await _catalogoRepositorio.ListarCategoriasConCantidadAsync();
            var talles = await _catalogoRepositorio.ListarTallesConCantidadAsync();
            var estados = await _catalogoRepositorio.ListarEstadosConCantidadAsync();

            return new OpcionesFiltroCatalogoDto
            {
                Categorias = categorias.Select(c => new CategoriaFiltroDto
                {
                    Id = c.Categoria.Id,
                    Nombre = c.Categoria.Nombre,
                    Cantidad = c.Cantidad
                }).ToList(),
                Talles = talles
                    .OrderBy(t => TalleConstantes.ObtenerIndice(t.Talle))
                    .ThenBy(t => t.Talle, StringComparer.Ordinal)
                    .Select(t => new OpcionFiltroDto
                    {
                        Valor = t.Talle,
                        Texto = t.Talle,
                        Cantidad = t.Cantidad
                    }).ToList(),
                Estados = estados
                    .OrderBy(e => (int)e.Estado)
                    .Select(e => new OpcionFiltroDto
                    {
                        Valor = e.Estado.ToString(),
                        Texto = EstadoPrendaHelper.ObtenerTexto(e.Estado),
                        Cantidad = e.Cantidad
                    }).ToList()
            };
        }

        private static CatalogoFiltrosDto NormalizarFiltros(CatalogoFiltrosDto filtros)
        {
            if (filtros is null)
                filtros = new CatalogoFiltrosDto();

            if (filtros.Page < 1)
                throw new ArgumentException("La página debe ser mayor o igual a 1.");
            if (filtros.PageSize < 1 || filtros.PageSize > CatalogoConstantes.PageSizeMaximo)
                throw new ArgumentException($"El pageSize debe estar entre 1 y {CatalogoConstantes.PageSizeMaximo}.");

            if (!string.IsNullOrWhiteSpace(filtros.Texto))
            {
                var texto = filtros.Texto.Trim();
                if (texto.Length > CatalogoConstantes.TextoMaximo)
                    throw new ArgumentException($"El texto no puede superar los {CatalogoConstantes.TextoMaximo} caracteres.");
                filtros.Texto = texto;
            }

            if (!string.IsNullOrWhiteSpace(filtros.Talle))
            {
                var talle = filtros.Talle.Trim();
                if (!TalleConstantes.EsValido(talle))
                    throw new ArgumentException("El talle del filtro es inválido.");
                filtros.Talle = talle;
            }

            if (filtros.Estado.HasValue && !Enum.IsDefined(filtros.Estado.Value))
                throw new ArgumentException("El estado del filtro es inválido.");

            if (!Enum.IsDefined(filtros.Orden))
                throw new ArgumentException("El orden del filtro es inválido.");

            return filtros;
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
                CategoriaId = producto.CategoriaId,
                ImagenUrl = producto.ImagenUrl,
                Talle = producto.Talle,
                Estado = producto.Estado.ToString(),
                EstadoTexto = EstadoPrendaHelper.ObtenerTexto(producto.Estado),
                Observaciones = producto.Observaciones,
                FechaAlta = DateTime.SpecifyKind(producto.FechaAlta, DateTimeKind.Utc),
                NombreCategoria = producto.Categoria?.Nombre ?? string.Empty
            };
        }
    }
}
