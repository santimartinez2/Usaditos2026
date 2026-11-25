using System;
using Usaditos2026.Shared.Constantes;

namespace Usaditos2026.Shared.Utilidades
{
    public static class EstadoPrendaHelper
    {
        public static string ObtenerTexto(EstadoPrenda estado)
        {
            return estado switch
            {
                EstadoPrenda.NuevoConEtiqueta => "Nuevo con etiqueta",
                EstadoPrenda.ComoNueva => "Como nueva",
                EstadoPrenda.MuyBueno => "Muy buen estado",
                EstadoPrenda.Bueno => "Buen estado",
                _ => throw new ArgumentOutOfRangeException(nameof(estado), estado, "El estado de la prenda es inválido.")
            };
        }
    }
}
