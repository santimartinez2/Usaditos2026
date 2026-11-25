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
    [Route("api/admin/pedidos")]
    public class AdminPedidosController : ControllerBase
    {
        private readonly IAdminPedidoServicio _adminPedidoServicio;

        public AdminPedidosController(IAdminPedidoServicio adminPedidoServicio)
        {
            _adminPedidoServicio = adminPedidoServicio;
        }

        [HttpGet]
        public async Task<ActionResult<PaginaDto<PedidoAdminResumenDto>>> Listar([FromQuery] PedidoAdminFiltrosDto filtros)
        {
            try
            {
                return Ok(await _adminPedidoServicio.ObtenerPedidosAsync(filtros));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<PedidoAdminDto>> Obtener(int id)
        {
            try
            {
                return Ok(await _adminPedidoServicio.ObtenerPedidoAsync(id));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
        }

        [HttpPost("{id:int}/estado")]
        public async Task<ActionResult<PedidoAdminDto>> CambiarEstado(
            int id,
            [FromBody] CambiarEstadoPedidoRequest request)
        {
            try
            {
                return Ok(await _adminPedidoServicio.CambiarEstadoAsync(id, request));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (EstadoPedidoInvalidoException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
            catch (PedidoModificadoException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
        }

        [HttpPost("{id:int}/pago")]
        public async Task<ActionResult<PedidoAdminDto>> MarcarPago(int id, [FromBody] MarcarPagoRequest request)
        {
            try
            {
                return Ok(await _adminPedidoServicio.MarcarPagoAsync(id, request));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (EstadoPedidoInvalidoException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
            catch (PedidoModificadoException ex)
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
