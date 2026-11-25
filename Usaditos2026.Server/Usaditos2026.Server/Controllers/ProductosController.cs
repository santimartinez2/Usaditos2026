using Microsoft.AspNetCore.Mvc;
using Usaditos2026.Servicios.Interfaces;
using Usaditos2026.Shared.Constantes;
using Usaditos2026.Shared.DTOs;

namespace Usaditos2026.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : ControllerBase
    {
        private readonly ICatalogoServicio _catalogoServicio;

        public ProductosController(ICatalogoServicio catalogoServicio)
        {
            _catalogoServicio = catalogoServicio;
        }

        [HttpGet]
        public async Task<ActionResult<List<ProductoDto>>> ObtenerProductos()
        {
            var productos = await _catalogoServicio.ObtenerProductosAsync();
            return Ok(productos);
        }

        [HttpGet("buscar")]
        public async Task<ActionResult<PaginaDto<ProductoDto>>> Buscar([FromQuery] CatalogoFiltrosDto filtros)
        {
            try
            {
                ValidarEnumsDelQuery();
                return Ok(await _catalogoServicio.BuscarProductosAsync(filtros));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpGet("filtros")]
        public async Task<ActionResult<OpcionesFiltroCatalogoDto>> ObtenerOpciones()
        {
            return Ok(await _catalogoServicio.ObtenerOpcionesFiltroAsync());
        }

        private void ValidarEnumsDelQuery()
        {
            ValidarEnumDelQuery<EstadoPrenda>("estado", "El estado del filtro es inválido.");
            ValidarEnumDelQuery<OrdenCatalogo>("orden", "El orden del filtro es inválido.");
        }

        private void ValidarEnumDelQuery<TEnum>(string nombre, string mensaje) where TEnum : struct, Enum
        {
            if (!Request.Query.TryGetValue(nombre, out var valores))
                return;

            var valor = valores.ToString().Trim();

            // El convertidor JSON no alcanza la query string: acá solo se aceptan nombres de enum.
            if (string.IsNullOrWhiteSpace(valor) ||
                int.TryParse(valor, out _) ||
                !Enum.TryParse<TEnum>(valor, ignoreCase: false, out var enumValue) ||
                !Enum.IsDefined(enumValue))
                throw new ArgumentException(mensaje);
        }
    }
}
