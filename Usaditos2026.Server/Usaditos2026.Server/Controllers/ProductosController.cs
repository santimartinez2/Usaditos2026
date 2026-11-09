using Microsoft.AspNetCore.Mvc;
using Usaditos2026.Servicios.Interfaces;
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
    }
}
