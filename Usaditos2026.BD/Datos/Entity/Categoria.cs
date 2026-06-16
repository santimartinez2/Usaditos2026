using System;
using System.Collections.Generic;
using System.Text;

namespace Usaditos2026.BD.Datos.Entity
{
    public class Categoria : EntityBase
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public ICollection<Producto> Productos { get; set; }
    }
}
