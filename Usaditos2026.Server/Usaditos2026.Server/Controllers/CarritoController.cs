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
    public class CarritoController : ControllerBase
    {
        private readonly ICarritoServicio _carritoServicio;

        public CarritoController(ICarritoServicio carritoServicio)
        {
            _carritoServicio = carritoServicio;
        }

        [HttpGet]
        public async Task<ActionResult<CarritoDto>> ObtenerCarrito()
        {
            if (!TryObtenerClienteId(out var clienteId))
                return Unauthorized();

            var carrito = await _carritoServicio.ObtenerCarritoAsync(clienteId);
            return Ok(carrito);
        }

        [HttpPost("items")]
        public async Task<ActionResult<CarritoDto>> AgregarProducto([FromBody] AgregarItemCarritoRequest request)
        {
            if (!TryObtenerClienteId(out var clienteId))
                return Unauthorized();

            try
            {
                var carrito = await _carritoServicio.AgregarProductoAsync(clienteId, request);
                return Ok(carrito);
            }
            catch (StockInsuficienteException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
            catch (LimiteUnidadesException ex)
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

        [HttpPut("items/{itemId:int}")]
        public async Task<ActionResult<CarritoDto>> ModificarCantidad(
            int itemId, [FromBody] ModificarCantidadRequest request)
        {
            if (!TryObtenerClienteId(out var clienteId))
                return Unauthorized();

            try
            {
                var carrito = await _carritoServicio.ModificarCantidadAsync(clienteId, itemId, request.Cantidad);
                return Ok(carrito);
            }
            catch (StockInsuficienteException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
            catch (LimiteUnidadesException ex)
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
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpDelete("items/{itemId:int}")]
        public async Task<ActionResult<CarritoDto>> EliminarProducto(int itemId)
        {
            if (!TryObtenerClienteId(out var clienteId))
                return Unauthorized();

            try
            {
                var carrito = await _carritoServicio.EliminarProductoAsync(clienteId, itemId);
                return Ok(carrito);
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

        [HttpDelete]
        public async Task<ActionResult<CarritoDto>> VaciarCarrito()
        {
            if (!TryObtenerClienteId(out var clienteId))
                return Unauthorized();

            var carrito = await _carritoServicio.VaciarCarritoAsync(clienteId);
            return Ok(carrito);
        }

        private bool TryObtenerClienteId(out int clienteId)
        {
            var valor = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(valor, out clienteId);
        }
    }
}
