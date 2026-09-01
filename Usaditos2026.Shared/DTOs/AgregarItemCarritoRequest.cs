namespace Usaditos2026.Shared.DTOs
{
    public class AgregarItemCarritoRequest
    {
        public int ClienteId { get; set; }
        public int ProductoId { get; set; }
        public int Cantidad { get; set; } = 1;
    }
}
