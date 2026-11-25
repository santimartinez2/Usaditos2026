using System.Collections.Generic;

namespace Usaditos2026.Shared.DTOs
{
    public class PaginaDto<T>
    {
        public List<T> Items { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
