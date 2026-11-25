using Usaditos2026.BD.Datos.Entity;
using Usaditos2026.Shared.Constantes;
using Usaditos2026.Shared.DTOs;

namespace Usaditos2026.Repositorio.Interfaces
{
    public interface ICatalogoRepositorio
    {
        Task<List<Producto>> ListarProductosActivosAsync();
        Task<(List<Producto> Productos, int Total)> BuscarAsync(CatalogoFiltrosDto filtros);
        Task<List<(Categoria Categoria, int Cantidad)>> ListarCategoriasConCantidadAsync();
        Task<List<(string Talle, int Cantidad)>> ListarTallesConCantidadAsync();
        Task<List<(EstadoPrenda Estado, int Cantidad)>> ListarEstadosConCantidadAsync();
    }
}
