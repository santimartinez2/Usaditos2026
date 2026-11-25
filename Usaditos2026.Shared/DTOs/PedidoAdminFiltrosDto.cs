using System;
using Usaditos2026.Shared.Constantes;

namespace Usaditos2026.Shared.DTOs
{
    public class PedidoAdminFiltrosDto
    {
        public string? Estado { get; set; }
        public EstadoPago? EstadoPago { get; set; }
        public MetodoPago? MetodoPago { get; set; }
        public TipoEntrega? TipoEntrega { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public string? Texto { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
