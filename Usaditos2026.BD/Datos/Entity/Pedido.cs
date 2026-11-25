using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Usaditos2026.Shared.Constantes;

namespace Usaditos2026.BD.Datos.Entity
{
    public class Pedido : EntityBase
    {
        public DateTime Fecha { get; set; }
        public string Estado { get; set; } = PedidoConstantes.EstadoConfirmado;
        public string TelefonoContacto { get; set; } = string.Empty;
        public TipoEntrega TipoEntrega { get; set; } = TipoEntrega.Retiro;
        public string? DireccionEnvio { get; set; }
        public string? NotaCliente { get; set; }
        public MetodoPago MetodoPago { get; set; } = MetodoPago.Efectivo;
        public EstadoPago EstadoPago { get; set; } = EstadoPago.Pendiente;
        public DateTime? FechaPago { get; set; }
        public decimal Total { get; set; }
        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; }
        public ICollection<ItemPedido> Items { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
