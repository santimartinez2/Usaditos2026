using System;
using Usaditos2026.Shared.Constantes;

namespace Usaditos2026.Shared.DTOs
{
    public class PedidoResumenDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string TelefonoContacto { get; set; } = string.Empty;
        public TipoEntrega TipoEntrega { get; set; } = TipoEntrega.Retiro;
        public string? DireccionEnvio { get; set; }
        public string? NotaCliente { get; set; }
        public MetodoPago MetodoPago { get; set; } = MetodoPago.Efectivo;
        public EstadoPago EstadoPago { get; set; } = EstadoPago.Pendiente;
        public DateTime? FechaPago { get; set; }
        public decimal Total { get; set; }
        public int CantidadItems { get; set; }
    }
}
