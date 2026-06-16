using System;
using System.Collections.Generic;
using System.Text;

namespace Usaditos2026.BD.Datos.Entity
{
    public class Producto : EntityBase
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int StockDisponible { get; set; }
        public bool Activo { get; set; }
        public int CategoriaId { get; set; }
        public Categoria Categoria { get; set; }
        public ICollection<ItemCarrito> ItemsCarrito { get; set; }
    }
}
