using System;
using System.Collections.Generic;
using System.Text;

namespace Usaditos2026.BD.Datos.Entity
{
    public class ItemCarrito : EntityBase
    {
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public int CarritoId { get; set; }
        public Carrito Carrito { get; set; }
        public int ProductoId { get; set; }
        public Producto Producto { get; set; }
    }
}
