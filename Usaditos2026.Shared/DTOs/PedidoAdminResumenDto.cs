using System;
using Usaditos2026.Shared.Constantes;

namespace Usaditos2026.Shared.DTOs
{
    public class PedidoAdminResumenDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Estado { get; set; } = string.Empty;
        public EstadoPago EstadoPago { get; set; }
        public MetodoPago MetodoPago { get; set; }
        public TipoEntrega TipoEntrega { get; set; }
        public decimal Total { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public string TelefonoContacto { get; set; } = string.Empty;
        public int CantidadItems { get; set; }
    }
}
