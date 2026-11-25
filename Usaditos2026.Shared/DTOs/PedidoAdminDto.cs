using System;
using System.Collections.Generic;
using Usaditos2026.Shared.Constantes;

namespace Usaditos2026.Shared.DTOs
{
    public class PedidoAdminDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Estado { get; set; } = string.Empty;
        public EstadoPago EstadoPago { get; set; }
        public MetodoPago MetodoPago { get; set; }
        public TipoEntrega TipoEntrega { get; set; }
        public string TelefonoContacto { get; set; } = string.Empty;
        public string? DireccionEnvio { get; set; }
        public string? NotaCliente { get; set; }
        public DateTime? FechaPago { get; set; }
        public decimal Total { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public string ClienteEmail { get; set; } = string.Empty;
        public string ClienteTelefono { get; set; } = string.Empty;
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
        public List<ItemPedidoDto> Items { get; set; } = new();
    }
}
