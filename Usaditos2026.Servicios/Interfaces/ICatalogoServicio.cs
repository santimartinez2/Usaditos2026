using Usaditos2026.Shared.Constantes;
using Usaditos2026.Shared.DTOs;

namespace Usaditos2026.Servicios.Interfaces
{
    public interface ICatalogoServicio
    {
        Task<List<ProductoDto>> ObtenerProductosAsync();
        Task<Dictionary<int, string?>> ObtenerImagenesAsync(IEnumerable<int> productoIds);
        Task<ProductoDto> ObtenerProductoAsync(int id);
        Task<List<ProductoDto>> ObtenerRelacionadosAsync(int id, int cantidad = CatalogoConstantes.RelacionadosPorDefecto);
        Task<PaginaDto<ProductoDto>> BuscarProductosAsync(CatalogoFiltrosDto filtros);
        Task<OpcionesFiltroCatalogoDto> ObtenerOpcionesFiltroAsync();
    }
}
