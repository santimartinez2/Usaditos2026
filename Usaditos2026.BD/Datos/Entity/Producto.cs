using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Usaditos2026.Shared.Constantes;

namespace Usaditos2026.BD.Datos.Entity
{
    public class Producto : EntityBase
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int StockDisponible { get; set; }
        public bool Activo { get; set; }
        public string? ImagenUrl { get; set; }
        public string Talle { get; set; } = string.Empty;
        public EstadoPrenda Estado { get; set; }
        public string? Observaciones { get; set; }
        public DateTime FechaAlta { get; set; }
        public int CategoriaId { get; set; }
        public Categoria Categoria { get; set; }
        public ICollection<ItemCarrito> ItemsCarrito { get; set; }
        public ICollection<ItemPedido> ItemsPedido { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
