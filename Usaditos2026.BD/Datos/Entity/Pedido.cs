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
        public decimal Total { get; set; }
        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; }
        public ICollection<ItemPedido> Items { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
