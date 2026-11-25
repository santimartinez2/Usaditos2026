using Usaditos2026.Shared.DTOs;

namespace Usaditos2026.Servicios.Interfaces
{
    public interface ICatalogoServicio
    {
        Task<List<ProductoDto>> ObtenerProductosAsync();
        Task<PaginaDto<ProductoDto>> BuscarProductosAsync(CatalogoFiltrosDto filtros);
        Task<OpcionesFiltroCatalogoDto> ObtenerOpcionesFiltroAsync();
    }
}
