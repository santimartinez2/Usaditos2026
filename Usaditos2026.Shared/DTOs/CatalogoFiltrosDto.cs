using Usaditos2026.Shared.Constantes;

namespace Usaditos2026.Shared.DTOs
{
    public class CatalogoFiltrosDto
    {
        public int? CategoriaId { get; set; }
        public string? Talle { get; set; }
        public EstadoPrenda? Estado { get; set; }
        public string? Texto { get; set; }
        public bool SoloDisponibles { get; set; }
        public OrdenCatalogo Orden { get; set; } = OrdenCatalogo.MasNuevos;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = CatalogoConstantes.PageSizePorDefecto;
    }
}
