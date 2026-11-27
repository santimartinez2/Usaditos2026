using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Usaditos2026.Shared.Excepciones;

namespace Usaditos2026.Server.Middleware
{
    /// <summary>
    /// Manejador global de excepciones para el pipeline HTTP (IExceptionHandler de .NET 10).
    /// Registra el error con ILogger y devuelve respuestas ProblemDetails estándar en español rioplatense
    /// sin exponer stack traces ni detalles internos en producción.
    /// </summary>
    public class ManejadorGlobalExcepciones : IExceptionHandler
    {
        private readonly ILogger<ManejadorGlobalExcepciones> _logger;
        private readonly IHostEnvironment _environment;

        public ManejadorGlobalExcepciones(
            ILogger<ManejadorGlobalExcepciones> logger,
            IHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception,
                "Error no controlado en la solicitud HTTP {Metodo} {Ruta}: {Mensaje}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                exception.Message);

            if (httpContext.Response.HasStarted)
            {
                _logger.LogWarning("No se pudo escribir la respuesta de error porque la respuesta HTTP ya había comenzado.");
                return false;
            }

            // Solo manejamos con ProblemDetails JSON las solicitudes hacia la API o que soliciten JSON.
            // Para solicitudes de páginas HTML de Blazor dejamos que el pipeline continúe hacia /Error o el manejador de componentes.
            var esApi = httpContext.Request.Path.StartsWithSegments("/api") ||
                        httpContext.Request.Headers.Accept.Any(h => h != null && h.Contains("application/json", StringComparison.OrdinalIgnoreCase));

            if (!esApi)
            {
                return false;
            }

            var (statusCode, titulo, detalle) = MapearExcepcion(exception);

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = titulo,
                Detail = detalle,
                Instance = httpContext.Request.Path
            };

            var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
            if (!string.IsNullOrEmpty(traceId))
            {
                problemDetails.Extensions["traceId"] = traceId;
            }

            if (_environment.IsDevelopment())
            {
                problemDetails.Extensions["excepcionTipo"] = exception.GetType().FullName;
                if (statusCode == StatusCodes.Status500InternalServerError)
                {
                    problemDetails.Extensions["stackTrace"] = exception.StackTrace;
                }
            }

            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/problem+json; charset=utf-8";

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken: cancellationToken);
            return true;
        }

        private static (int StatusCode, string Titulo, string Detalle) MapearExcepcion(Exception exception)
        {
            return exception switch
            {
                // Concurrencia (PedidoModificadoException, ProductoModificadoException, DbUpdateConcurrencyException) -> 409 Conflict
                PedidoModificadoException or ProductoModificadoException or DbUpdateConcurrencyException => (
                    StatusCodes.Status409Conflict,
                    "Conflicto de concurrencia",
                    "El registro fue modificado por otro usuario, recargá e intentá de nuevo."
                ),

                // Sin sesión (UnauthorizedAccessException) -> 401 Unauthorized
                UnauthorizedAccessException => (
                    StatusCodes.Status401Unauthorized,
                    "No autorizado",
                    string.IsNullOrWhiteSpace(exception.Message) || exception.Message == new UnauthorizedAccessException().Message
                        ? "No iniciaste sesión o la sesión expiró."
                        : exception.Message
                ),

                // Sin permiso (SecurityException o acceso denegado) -> 403 Forbidden
                System.Security.SecurityException => (
                    StatusCodes.Status403Forbidden,
                    "Acceso denegado",
                    string.IsNullOrWhiteSpace(exception.Message) || exception.Message == new System.Security.SecurityException().Message
                        ? "No tenés permisos para realizar esta acción."
                        : exception.Message
                ),

                // No existe (KeyNotFoundException) -> 404 Not Found
                KeyNotFoundException => (
                    StatusCodes.Status404NotFound,
                    "No encontrado",
                    string.IsNullOrWhiteSpace(exception.Message) || exception.Message == new KeyNotFoundException().Message
                        ? "El recurso solicitado no fue encontrado."
                        : exception.Message
                ),

                // Validación / reglas de negocio (ArgumentException, StockInsuficienteException, CarritoVacioException, EstadoPedidoInvalidoException, InvalidOperationException) -> 400 Bad Request
                ArgumentException or
                StockInsuficienteException or
                CarritoVacioException or
                EstadoPedidoInvalidoException or
                InvalidOperationException or
                CategoriaDuplicadaException or
                CategoriaConProductosException => (
                    StatusCodes.Status400BadRequest,
                    "Solicitud incorrecta",
                    exception.Message
                ),

                // Cualquier otra excepción inesperada -> 500 Internal Server Error
                _ => (
                    StatusCodes.Status500InternalServerError,
                    "Error interno del servidor",
                    "Ocurrió un error inesperado. Por favor, intentá de nuevo en unos instantes."
                )
            };
        }
    }
}
