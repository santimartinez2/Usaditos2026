using System.Collections.Generic;

namespace Usaditos2026.Shared.DTOs
{
    public class OpcionesFiltroCatalogoDto
    {
        public List<CategoriaFiltroDto> Categorias { get; set; } = new();
        public List<OpcionFiltroDto> Talles { get; set; } = new();
        public List<OpcionFiltroDto> Estados { get; set; } = new();
    }

    public class CategoriaFiltroDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
    }

    public class OpcionFiltroDto
    {
        public string Valor { get; set; } = string.Empty;
        public string Texto { get; set; } = string.Empty;
        public int Cantidad { get; set; }
    }
}
