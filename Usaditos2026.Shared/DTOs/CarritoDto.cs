namespace Usaditos2026.Shared.DTOs
{
    public class CarritoDto
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public List<ItemCarritoDto> Items { get; set; } = new();
        public decimal Total => Items.Sum(i => i.Subtotal);
    }
}
