using System.ComponentModel.DataAnnotations;
using Usaditos2026.Shared.Constantes;

namespace Usaditos2026.Shared.DTOs
{
    public class ConfirmarPedidoRequest
    {
        [Required(ErrorMessage = "El teléfono de contacto es obligatorio.")]
        public string TelefonoContacto { get; set; } = string.Empty;

        public TipoEntrega? TipoEntrega { get; set; }

        public string? DireccionEnvio { get; set; }

        public MetodoPago? MetodoPago { get; set; }

        public string? NotaCliente { get; set; }
    }
}
