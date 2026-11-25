using Usaditos2026.Shared.DTOs;

namespace Usaditos2026.Servicios.Interfaces
{
    public interface IAdminCatalogoServicio
    {
        Task<List<CategoriaDto>> ObtenerCategoriasAsync();
        Task<CategoriaDto> CrearCategoriaAsync(CrearCategoriaRequest request);
        Task<CategoriaDto> ModificarCategoriaAsync(int id, ModificarCategoriaRequest request);
        Task EliminarCategoriaAsync(int id);

        Task<List<ProductoAdminDto>> ObtenerProductosAsync();
        Task<ProductoAdminDto> ObtenerProductoAsync(int id);
        Task<ProductoAdminDto> CrearProductoAsync(CrearProductoRequest request);
        Task<ProductoAdminDto> ModificarProductoAsync(int id, ModificarProductoRequest request);
        Task DesactivarProductoAsync(int id, byte[] rowVersion);
        Task<ProductoAdminDto> ActivarProductoAsync(int id, byte[] rowVersion);
        Task<ProductoAdminDto> AsignarImagenAsync(
            int id, Stream contenido, long tamaño, string nombreArchivo, byte[] rowVersion);
        Task<ProductoAdminDto> QuitarImagenAsync(int id, byte[] rowVersion);
    }
}
