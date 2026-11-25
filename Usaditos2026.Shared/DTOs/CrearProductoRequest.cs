using System.ComponentModel.DataAnnotations;
using Usaditos2026.Shared.Constantes;

namespace Usaditos2026.Shared.DTOs
{
    public class CrearProductoRequest
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombre { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public decimal Precio { get; set; }

        public int StockDisponible { get; set; }

        public int CategoriaId { get; set; }
        public string Talle { get; set; } = string.Empty;
        public EstadoPrenda? Estado { get; set; }
        public string? Observaciones { get; set; }
    }
}
