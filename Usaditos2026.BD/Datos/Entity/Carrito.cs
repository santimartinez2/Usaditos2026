using System;
using System.Collections.Generic;
using System.Text;
using Usaditos2026.Shared.Constantes;

namespace Usaditos2026.BD.Datos.Entity
{
    public class Carrito : EntityBase
    {
        public DateTime FechaCreacion { get; set; }
        public string Estado { get; set; } = CarritoConstantes.EstadoActivo;
        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; }
        public ICollection<ItemCarrito> Items { get; set; }
    }
}
