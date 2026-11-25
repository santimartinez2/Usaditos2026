using System.ComponentModel.DataAnnotations;

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
    }
}
