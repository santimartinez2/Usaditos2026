using System;
using Usaditos2026.Shared.Constantes;

namespace Usaditos2026.Shared.Utilidades
{
    public static class TalleTextoHelper
    {
        public static string Formatear(string? talle)
        {
            if (string.IsNullOrWhiteSpace(talle))
                return TalleConstantes.TalleUnico;

            var normalizado = talle.Trim();

            // Si es "Talle único", se devuelve tal cual (nunca "Talle Talle único")
            if (normalizado.Equals(TalleConstantes.TalleUnico, StringComparison.OrdinalIgnoreCase))
                return TalleConstantes.TalleUnico;

            // Si es calzado (ej. "Calzado 18", "Calzado 24"), se devuelve tal cual (nunca "Talle Calzado 24")
            if (normalizado.StartsWith("Calzado", StringComparison.OrdinalIgnoreCase))
                return normalizado;

            // Si ya tiene el prefijo "Talle " (ej. "Talle 4"), se devuelve tal cual
            if (normalizado.StartsWith("Talle ", StringComparison.OrdinalIgnoreCase))
                return normalizado;

            // Para talles de ropa (RN, rangos de meses, números): "Talle X"
            return $"Talle {normalizado}";
        }

        public static string ObtenerTexto(string? talle) => Formatear(talle);
    }
}
