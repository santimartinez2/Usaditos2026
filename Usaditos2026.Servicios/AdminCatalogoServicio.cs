using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Usaditos2026.BD.Datos.Entity;
using Usaditos2026.Repositorio.Interfaces;
using Usaditos2026.Servicios.Imagenes;
using Usaditos2026.Servicios.Interfaces;
using Usaditos2026.Shared.Constantes;
using Usaditos2026.Shared.DTOs;
using Usaditos2026.Shared.Excepciones;

namespace Usaditos2026.Servicios
{
    public class AdminCatalogoServicio : IAdminCatalogoServicio
    {
        private readonly IAdminCatalogoRepositorio _adminCatalogoRepositorio;
        private readonly IAlmacenImagenes _almacenImagenes;
        private readonly ImagenesOpciones _imagenesOpciones;
        private readonly ILogger<AdminCatalogoServicio> _logger;

        public AdminCatalogoServicio(
            IAdminCatalogoRepositorio adminCatalogoRepositorio,
            IAlmacenImagenes almacenImagenes,
            IOptions<ImagenesOpciones> imagenesOpciones,
            ILogger<AdminCatalogoServicio> logger)
        {
            _adminCatalogoRepositorio = adminCatalogoRepositorio;
            _almacenImagenes = almacenImagenes;
            _imagenesOpciones = imagenesOpciones.Value;
            _logger = logger;
        }

        public async Task<List<CategoriaDto>> ObtenerCategoriasAsync()
        {
            var categorias = await _adminCatalogoRepositorio.ListarCategoriasAsync();
            return categorias.Select(MapearACategoriaDto).ToList();
        }

        public async Task<CategoriaDto> CrearCategoriaAsync(CrearCategoriaRequest request)
        {
            var nombre = ValidarNombreCategoria(request.Nombre);
            await ValidarCategoriaDuplicadaAsync(nombre);

            var categoria = new Categoria
            {
                Nombre = nombre,
                Descripcion = request.Descripcion?.Trim() ?? string.Empty
            };

            await _adminCatalogoRepositorio.AgregarCategoriaAsync(categoria);
            await _adminCatalogoRepositorio.GuardarCambiosAsync();

            return MapearACategoriaDto(categoria);
        }

        public async Task<CategoriaDto> ModificarCategoriaAsync(int id, ModificarCategoriaRequest request)
        {
            var categoria = await _adminCatalogoRepositorio.ObtenerCategoriaPorIdAsync(id)
                ?? throw new KeyNotFoundException("La categoría no existe.");

            var nombre = ValidarNombreCategoria(request.Nombre);
            await ValidarCategoriaDuplicadaAsync(nombre, id);

            categoria.Nombre = nombre;
            categoria.Descripcion = request.Descripcion?.Trim() ?? string.Empty;

            await _adminCatalogoRepositorio.GuardarCambiosAsync();

            return MapearACategoriaDto(categoria);
        }

        public async Task EliminarCategoriaAsync(int id)
        {
            var categoria = await _adminCatalogoRepositorio.ObtenerCategoriaPorIdAsync(id)
                ?? throw new KeyNotFoundException("La categoría no existe.");

            if (await _adminCatalogoRepositorio.CategoriaTieneProductosAsync(id))
                throw new CategoriaConProductosException(
                    $"No se puede eliminar la categoría '{categoria.Nombre}' porque tiene productos asociados.");

            await _adminCatalogoRepositorio.EliminarCategoriaAsync(categoria);
            await _adminCatalogoRepositorio.GuardarCambiosAsync();
        }

        public async Task<List<ProductoAdminDto>> ObtenerProductosAsync()
        {
            var productos = await _adminCatalogoRepositorio.ListarProductosAsync();
            return productos.Select(MapearAProductoAdminDto).ToList();
        }

        public async Task<ProductoAdminDto> ObtenerProductoAsync(int id)
        {
            var producto = await _adminCatalogoRepositorio.ObtenerProductoAsync(id)
                ?? throw new KeyNotFoundException("El producto no existe.");

            return MapearAProductoAdminDto(producto);
        }

        public async Task<ProductoAdminDto> CrearProductoAsync(CrearProductoRequest request)
        {
            var datos = ValidarDatosProducto(
                request.Nombre, request.Descripcion, request.Precio, request.StockDisponible, request.CategoriaId);
            await ValidarCategoriaExisteAsync(datos.CategoriaId);

            var producto = new Producto
            {
                Nombre = datos.Nombre,
                Descripcion = datos.Descripcion,
                Precio = datos.Precio,
                StockDisponible = datos.Stock,
                CategoriaId = datos.CategoriaId,
                Activo = true
            };

            await _adminCatalogoRepositorio.AgregarProductoAsync(producto);
            await _adminCatalogoRepositorio.GuardarCambiosAsync();

            var creado = await _adminCatalogoRepositorio.ObtenerProductoAsync(producto.Id);
            return MapearAProductoAdminDto(creado!);
        }

        public async Task<ProductoAdminDto> ModificarProductoAsync(int id, ModificarProductoRequest request)
        {
            var producto = await _adminCatalogoRepositorio.ObtenerProductoParaEdicionAsync(id)
                ?? throw new KeyNotFoundException("El producto no existe.");

            var datos = ValidarDatosProducto(
                request.Nombre, request.Descripcion, request.Precio, request.StockDisponible, request.CategoriaId);
            await ValidarCategoriaExisteAsync(datos.CategoriaId);
            ValidarRowVersion(request.RowVersion);

            producto.Nombre = datos.Nombre;
            producto.Descripcion = datos.Descripcion;
            producto.Precio = datos.Precio;
            producto.StockDisponible = datos.Stock;
            producto.CategoriaId = datos.CategoriaId;

            await _adminCatalogoRepositorio.EstablecerRowVersionOriginalAsync(producto, request.RowVersion);
            await GuardarConConcurrenciaAsync();

            var actualizado = await _adminCatalogoRepositorio.ObtenerProductoAsync(id);
            return MapearAProductoAdminDto(actualizado!);
        }

        public async Task DesactivarProductoAsync(int id, byte[] rowVersion)
        {
            var producto = await _adminCatalogoRepositorio.ObtenerProductoParaEdicionAsync(id)
                ?? throw new KeyNotFoundException("El producto no existe.");

            ValidarRowVersion(rowVersion);
            producto.Activo = false;

            await _adminCatalogoRepositorio.EstablecerRowVersionOriginalAsync(producto, rowVersion);
            await GuardarConConcurrenciaAsync();
        }

        public async Task<ProductoAdminDto> ActivarProductoAsync(int id, byte[] rowVersion)
        {
            var producto = await _adminCatalogoRepositorio.ObtenerProductoParaEdicionAsync(id)
                ?? throw new KeyNotFoundException("El producto no existe.");

            ValidarRowVersion(rowVersion);
            producto.Activo = true;

            await _adminCatalogoRepositorio.EstablecerRowVersionOriginalAsync(producto, rowVersion);
            await GuardarConConcurrenciaAsync();

            var actualizado = await _adminCatalogoRepositorio.ObtenerProductoAsync(id);
            return MapearAProductoAdminDto(actualizado!);
        }

        public async Task<ProductoAdminDto> AsignarImagenAsync(
            int id, Stream contenido, long tamaño, string nombreArchivo, byte[] rowVersion)
        {
            ValidarRowVersion(rowVersion);
            ValidarTamaño(tamaño, contenido);

            // El nombre del cliente nunca se usa: el servidor genera un GUID con la extensión detectada.
            var imagen = await ImagenProcesador.ProcesarAsync(contenido);
            var nombreServidor = $"{Guid.NewGuid():N}.{imagen.Extension}";
            var producto = await _adminCatalogoRepositorio.ObtenerProductoParaEdicionAsync(id)
                ?? throw new KeyNotFoundException("El producto no existe.");
            var imagenAnterior = producto.ImagenUrl;

            // El archivo se guarda primero; si falla la base, se descarta el archivo nuevo.
            var nuevaUrl = await _almacenImagenes.GuardarAsync(new MemoryStream(imagen.Contenido), nombreServidor);
            var guardado = false;
            try
            {
                producto.ImagenUrl = nuevaUrl;
                await _adminCatalogoRepositorio.EstablecerRowVersionOriginalAsync(producto, rowVersion);
                await GuardarConConcurrenciaAsync();
                guardado = true;
            }
            finally
            {
                if (!guardado)
                    await BorrarSinRomperAsync(nuevaUrl);
            }

            // Recién después de guardar en la base se quita el archivo anterior.
            if (!string.IsNullOrWhiteSpace(imagenAnterior) && imagenAnterior != nuevaUrl)
                await BorrarSinRomperAsync(imagenAnterior);

            var actualizado = await _adminCatalogoRepositorio.ObtenerProductoAsync(id);
            return MapearAProductoAdminDto(actualizado!);
        }

        public async Task<ProductoAdminDto> QuitarImagenAsync(int id, byte[] rowVersion)
        {
            ValidarRowVersion(rowVersion);

            var producto = await _adminCatalogoRepositorio.ObtenerProductoParaEdicionAsync(id)
                ?? throw new KeyNotFoundException("El producto no existe.");
            var imagenAnterior = producto.ImagenUrl;

            if (!string.IsNullOrWhiteSpace(imagenAnterior))
            {
                producto.ImagenUrl = null;
                await _adminCatalogoRepositorio.EstablecerRowVersionOriginalAsync(producto, rowVersion);
                await GuardarConConcurrenciaAsync();
                await BorrarSinRomperAsync(imagenAnterior);
            }

            var actualizado = await _adminCatalogoRepositorio.ObtenerProductoAsync(id);
            return MapearAProductoAdminDto(actualizado!);
        }

        private void ValidarTamaño(long tamaño, Stream contenido)
        {
            var mb = _imagenesOpciones.TamanoMaximoMb > 0
                ? _imagenesOpciones.TamanoMaximoMb
                : ImagenesConstantes.TamanoMaximoPorDefectoMb;
            var maximo = mb * 1024L * 1024L;

            if (tamaño <= 0 || tamaño > maximo)
                throw new ArgumentException($"La imagen es obligatoria y no puede superar los {mb} MB.");

            if (contenido.CanSeek && contenido.Length > maximo)
                throw new ArgumentException($"La imagen no puede superar los {mb} MB.");
        }

        private async Task BorrarSinRomperAsync(string? ruta)
        {
            if (string.IsNullOrWhiteSpace(ruta))
                return;

            try
            {
                await _almacenImagenes.BorrarAsync(ruta);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo borrar la imagen {ImagenUrl}.", ruta);
            }
        }

        private async Task GuardarConConcurrenciaAsync()
        {
            try
            {
                await _adminCatalogoRepositorio.GuardarCambiosAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ProductoModificadoException("El producto fue modificado, recargá.");
            }
        }

        private static string ValidarNombreCategoria(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre de la categoría es obligatorio.");

            return nombre.Trim();
        }

        private async Task ValidarCategoriaDuplicadaAsync(string nombre, int? idExcluido = null)
        {
            var existente = await _adminCatalogoRepositorio.ObtenerCategoriaPorNombreAsync(nombre);

            if (existente is not null && existente.Id != idExcluido)
                throw new CategoriaDuplicadaException($"Ya existe una categoría con el nombre '{nombre}'.");
        }

        private async Task ValidarCategoriaExisteAsync(int categoriaId)
        {
            if (await _adminCatalogoRepositorio.ObtenerCategoriaPorIdAsync(categoriaId) is null)
                throw new KeyNotFoundException("La categoría no existe.");
        }

        private static (string Nombre, string Descripcion, decimal Precio, int Stock, int CategoriaId) ValidarDatosProducto(
            string nombre, string descripcion, decimal precio, int stock, int categoriaId)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre del producto es obligatorio.");

            var nombreNormalizado = nombre.Trim();

            if (nombreNormalizado.Length > 200)
                throw new ArgumentException("El nombre del producto no puede superar los 200 caracteres.");

            if (precio <= 0)
                throw new ArgumentException("El precio debe ser mayor a cero.");

            if (stock < 0)
                throw new ArgumentException("El stock no puede ser negativo.");

            if (categoriaId <= 0)
                throw new ArgumentException("La categoría es obligatoria.");

            return (nombreNormalizado, descripcion?.Trim() ?? string.Empty, precio, stock, categoriaId);
        }

        private static void ValidarRowVersion(byte[] rowVersion)
        {
            if (rowVersion is null || rowVersion.Length == 0)
                throw new ArgumentException("El RowVersion es obligatorio. Obtenelo con GET /api/admin/productos/{id}.");
        }

        private static CategoriaDto MapearACategoriaDto(Categoria categoria)
        {
            return new CategoriaDto
            {
                Id = categoria.Id,
                Nombre = categoria.Nombre,
                Descripcion = categoria.Descripcion ?? string.Empty
            };
        }

        private static ProductoAdminDto MapearAProductoAdminDto(Producto producto)
        {
            return new ProductoAdminDto
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion ?? string.Empty,
                Precio = producto.Precio,
                StockDisponible = producto.StockDisponible,
                Activo = producto.Activo,
                CategoriaId = producto.CategoriaId,
                ImagenUrl = producto.ImagenUrl,
                NombreCategoria = producto.Categoria?.Nombre ?? string.Empty,
                RowVersion = producto.RowVersion ?? Array.Empty<byte>()
            };
        }
    }
}
