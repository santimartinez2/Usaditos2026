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
    [Route("api/admin/categorias")]
    public class AdminCategoriasController : ControllerBase
    {
        private readonly IAdminCatalogoServicio _adminCatalogoServicio;

        public AdminCategoriasController(IAdminCatalogoServicio adminCatalogoServicio)
        {
            _adminCatalogoServicio = adminCatalogoServicio;
        }

        [HttpGet]
        public async Task<ActionResult<List<CategoriaDto>>> Listar()
        {
            return Ok(await _adminCatalogoServicio.ObtenerCategoriasAsync());
        }

        [HttpPost]
        public async Task<ActionResult<CategoriaDto>> Crear([FromBody] CrearCategoriaRequest request)
        {
            try
            {
                var categoria = await _adminCatalogoServicio.CrearCategoriaAsync(request);
                return StatusCode(StatusCodes.Status201Created, categoria);
            }
            catch (CategoriaDuplicadaException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<CategoriaDto>> Modificar(int id, [FromBody] ModificarCategoriaRequest request)
        {
            try
            {
                return Ok(await _adminCatalogoServicio.ModificarCategoriaAsync(id, request));
            }
            catch (CategoriaDuplicadaException ex)
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
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                await _adminCatalogoServicio.EliminarCategoriaAsync(id);
                return NoContent();
            }
            catch (CategoriaConProductosException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
        }
    }
}
