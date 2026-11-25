namespace Usaditos2026.Shared.DTOs
{
    public class PedidoDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Estado { get; set; } = string.Empty;
        public List<ItemPedidoDto> Items { get; set; } = new();
        public decimal Total => Items.Sum(i => i.Subtotal);
    }
}
