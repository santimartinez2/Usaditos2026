using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Usaditos2026.Servicios.Interfaces;
using Usaditos2026.Shared.DTOs;
using Usaditos2026.Shared.Excepciones;

namespace Usaditos2026.Server.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class PedidosController : ControllerBase
    {
        private readonly IPedidoServicio _pedidoServicio;

        public PedidosController(IPedidoServicio pedidoServicio)
        {
            _pedidoServicio = pedidoServicio;
        }

        [HttpPost]
        public async Task<ActionResult<PedidoDto>> ConfirmarPedido()
        {
            if (!TryObtenerClienteId(out var clienteId))
                return Unauthorized();

            try
            {
                var pedido = await _pedidoServicio.ConfirmarPedidoAsync(clienteId);
                return CreatedAtAction(nameof(ObtenerPedido), new { id = pedido.Id }, pedido);
            }
            catch (CarritoVacioException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
            catch (StockInsuficienteException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<PedidoResumenDto>>> ObtenerMisPedidos()
        {
            if (!TryObtenerClienteId(out var clienteId))
                return Unauthorized();

            var pedidos = await _pedidoServicio.ObtenerMisPedidosAsync(clienteId);
            return Ok(pedidos);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<PedidoDto>> ObtenerPedido(int id)
        {
            if (!TryObtenerClienteId(out var clienteId))
                return Unauthorized();

            try
            {
                var pedido = await _pedidoServicio.ObtenerPedidoAsync(clienteId, id);
                return Ok(pedido);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { mensaje = ex.Message });
            }
        }

        [HttpPost("{id:int}/cancelar")]
        public async Task<ActionResult<PedidoDto>> CancelarPedido(int id)
        {
            if (!TryObtenerClienteId(out var clienteId))
                return Unauthorized();

            try
            {
                var pedido = await _pedidoServicio.CancelarPedidoAsync(clienteId, id);
                return Ok(pedido);
            }
            catch (EstadoPedidoInvalidoException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { mensaje = ex.Message });
            }
        }

        private bool TryObtenerClienteId(out int clienteId)
        {
            var valor = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(valor, out clienteId);
        }
    }
}
