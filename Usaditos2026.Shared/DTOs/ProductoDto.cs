namespace Usaditos2026.Shared.DTOs
{
    public class ProductoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int StockDisponible { get; set; }
        public string? ImagenUrl { get; set; }
        public string Talle { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string EstadoTexto { get; set; } = string.Empty;
        public string? Observaciones { get; set; }
        public DateTime FechaAlta { get; set; }
        public string NombreCategoria { get; set; } = string.Empty;
    }
}
