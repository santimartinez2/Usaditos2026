using System;
using System.Collections.Generic;
using System.Linq;

namespace Usaditos2026.Shared.Constantes
{
    public static class TalleConstantes
    {
        public const string TalleUnico = "Talle único";
        public const int LongitudMaxima = 20;

        // El orden de esta lista se usa también para mostrar los filtros.
        public static readonly IReadOnlyList<string> Valores = new[]
        {
            "RN",
            "0-3 meses",
            "3-6 meses",
            "6-9 meses",
            "9-12 meses",
            "12-18 meses",
            "18-24 meses",
            "2",
            "3",
            "4",
            "5",
            "6",
            "8",
            "10",
            "12",
            "14",
            "16",
            TalleUnico,
            "Calzado 18",
            "Calzado 19",
            "Calzado 20",
            "Calzado 21",
            "Calzado 22",
            "Calzado 23",
            "Calzado 24",
            "Calzado 25",
            "Calzado 26",
            "Calzado 27",
            "Calzado 28",
            "Calzado 29",
            "Calzado 30",
            "Calzado 31",
            "Calzado 32",
            "Calzado 33",
            "Calzado 34",
            "Calzado 35"
        };

        public static bool EsValido(string? talle)
        {
            if (string.IsNullOrWhiteSpace(talle))
                return false;

            var normalizado = talle.Trim();
            return Valores.Any(valor => valor == normalizado);
        }

        public static int ObtenerIndice(string talle)
        {
            return Array.IndexOf(Valores.ToArray(), talle);
        }
    }
}
