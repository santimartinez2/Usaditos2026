using System;
using System.ComponentModel.DataAnnotations;

namespace Usaditos2026.Shared.DTOs
{
    public class CambiarEstadoPedidoRequest
    {
        [Required(ErrorMessage = "El nuevo estado es obligatorio.")]
        public string NuevoEstado { get; set; } = string.Empty;

        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
