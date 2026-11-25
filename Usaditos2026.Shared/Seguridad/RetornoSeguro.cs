using System;
using System.Linq;

namespace Usaditos2026.Shared.Seguridad
{
    public static class RetornoSeguro
    {
        // Solo se aceptan rutas locales: /catalogo, /carrito, etc.
        public static string ObtenerDestinoSeguro(string? returnUrl)
        {
            if (string.IsNullOrWhiteSpace(returnUrl))
                return "/";

            if (!returnUrl.StartsWith('/')
                || returnUrl.StartsWith("//", StringComparison.Ordinal)
                || returnUrl.Contains('\\')
                || returnUrl.Any(char.IsControl))
                return "/";

            return returnUrl;
        }
    }
}
