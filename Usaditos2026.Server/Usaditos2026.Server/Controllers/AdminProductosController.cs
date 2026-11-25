using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Usaditos2026.Servicios.Interfaces;
using Usaditos2026.Shared.Constantes;
using Usaditos2026.Shared.DTOs;
using Usaditos2026.Shared.Excepciones;

namespace Usaditos2026.Server.Controllers
{
    [ApiController]
    [Authorize(Policy = AutorizacionConstantes.PoliticaSoloAdmin)]
    [Route("api/admin/productos")]
    public class AdminProductosController : ControllerBase
    {
        private readonly IAdminCatalogoServicio _adminCatalogoServicio;

        public AdminProductosController(IAdminCatalogoServicio adminCatalogoServicio)
        {
            _adminCatalogoServicio = adminCatalogoServicio;
        }

        [HttpGet]
        public async Task<ActionResult<List<ProductoAdminDto>>> Listar()
        {
            return Ok(await _adminCatalogoServicio.ObtenerProductosAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductoAdminDto>> Obtener(int id)
        {
            try
            {
                return Ok(await _adminCatalogoServicio.ObtenerProductoAsync(id));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ProductoAdminDto>> Crear([FromBody] CrearProductoRequest request)
        {
            try
            {
                var producto = await _adminCatalogoServicio.CrearProductoAsync(request);
                return CreatedAtAction(nameof(Obtener), new { id = producto.Id }, producto);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ProductoAdminDto>> Modificar(int id, [FromBody] ModificarProductoRequest request)
        {
            try
            {
                return Ok(await _adminCatalogoServicio.ModificarProductoAsync(id, request));
            }
            catch (ProductoModificadoException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Desactivar(int id, [FromBody] CambiarEstadoProductoRequest request)
        {
            try
            {
                await _adminCatalogoServicio.DesactivarProductoAsync(id, request.RowVersion);
                return NoContent();
            }
            catch (ProductoModificadoException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPost("{id:int}/activar")]
        public async Task<ActionResult<ProductoAdminDto>> Activar(int id, [FromBody] CambiarEstadoProductoRequest request)
        {
            try
            {
                return Ok(await _adminCatalogoServicio.ActivarProductoAsync(id, request.RowVersion));
            }
            catch (ProductoModificadoException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }
}
