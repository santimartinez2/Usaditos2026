using System;
using Usaditos2026.Shared.Constantes;

namespace Usaditos2026.Shared.DTOs
{
    public class MarcarPagoRequest
    {
        public EstadoPago? EstadoPago { get; set; }

        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
