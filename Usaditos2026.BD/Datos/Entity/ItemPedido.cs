using System;
using System.Collections.Generic;
using System.Text;

namespace Usaditos2026.BD.Datos.Entity
{
    public class ItemPedido : EntityBase
    {
        public int PedidoId { get; set; }
        public Pedido Pedido { get; set; }
        public int ProductoId { get; set; }
        public Producto Producto { get; set; }
        public string NombreProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
    }
}
