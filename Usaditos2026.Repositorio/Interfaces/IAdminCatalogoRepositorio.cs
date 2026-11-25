using Usaditos2026.BD.Datos.Entity;

namespace Usaditos2026.Repositorio.Interfaces
{
    public interface IAdminCatalogoRepositorio
    {
        Task<List<Categoria>> ListarCategoriasAsync();
        Task<Categoria?> ObtenerCategoriaPorIdAsync(int id);
        Task<Categoria?> ObtenerCategoriaPorNombreAsync(string nombre);
        Task AgregarCategoriaAsync(Categoria categoria);
        Task EliminarCategoriaAsync(Categoria categoria);
        Task<bool> CategoriaTieneProductosAsync(int categoriaId);

        Task<List<Producto>> ListarProductosAsync();
        Task<Producto?> ObtenerProductoAsync(int id);
        Task<Producto?> ObtenerProductoParaEdicionAsync(int id);
        Task AgregarProductoAsync(Producto producto);
        Task EstablecerRowVersionOriginalAsync(Producto producto, byte[] rowVersion);
        Task GuardarCambiosAsync();
    }
}
