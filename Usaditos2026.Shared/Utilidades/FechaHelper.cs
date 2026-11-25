using System;

namespace Usaditos2026.Shared.Utilidades
{
    public static class FechaHelper
    {
        public const string ZonaPorDefecto = "America/Argentina/Cordoba";
        private const string ZonaWindows = "Argentina Standard Time";

        public static TimeZoneInfo ObtenerZona(string? zonaId)
        {
            var candidatos = new[] { zonaId, ZonaPorDefecto, ZonaWindows };

            foreach (var candidato in candidatos)
            {
                if (string.IsNullOrWhiteSpace(candidato))
                    continue;

                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(candidato.Trim());
                }
                catch (TimeZoneNotFoundException)
                {
                }
                catch (InvalidTimeZoneException)
                {
                }
            }

            return TimeZoneInfo.Utc;
        }

        public static DateTime LocalAUtc(DateTime fechaLocal, string? zonaId)
        {
            // Un valor con "Z" u offset cambiaría el resultado de ConvertTimeToUtc: se trata como fecha local pura.
            var fecha = DateTime.SpecifyKind(fechaLocal, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeToUtc(fecha, ObtenerZona(zonaId));
        }

        public static DateTime UtcALocal(DateTime fechaUtc, string? zonaId)
        {
            var fecha = DateTime.SpecifyKind(fechaUtc, DateTimeKind.Utc);
            return TimeZoneInfo.ConvertTimeFromUtc(fecha, ObtenerZona(zonaId));
        }
    }
}
