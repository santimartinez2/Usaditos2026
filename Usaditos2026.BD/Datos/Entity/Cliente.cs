using System;
using System.Collections.Generic;
using System.Text;

namespace Usaditos2026.BD.Datos.Entity
{
    public class Cliente : EntityBase
    {
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public DateTime FechaRegistro { get; set; }
        public bool Activo { get; set; }
        public ICollection<Carrito> Carritos { get; set; }
    }
}
