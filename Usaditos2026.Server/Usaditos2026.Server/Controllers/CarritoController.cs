using Microsoft.AspNetCore.Mvc;
using Usaditos2026.Servicios.Interfaces;
using Usaditos2026.Shared.DTOs;
using Usaditos2026.Shared.Excepciones;

namespace Usaditos2026.Server.Controllers
{
   
    [ApiController]
    [Route("api/[controller]")]
    public class CarritoController : ControllerBase
    {
        private readonly ICarritoServicio _carritoServicio;

        public CarritoController(ICarritoServicio carritoServicio)
        {
            _carritoServicio = carritoServicio;
        }

       
        [HttpGet("{clienteId:int}")]
        public async Task<ActionResult<CarritoDto>> ObtenerCarrito(int clienteId)
        {
            var carrito = await _carritoServicio.ObtenerCarritoAsync(clienteId);
            return Ok(carrito);
        }

        
        [HttpPost("items")]
        public async Task<ActionResult<CarritoDto>> AgregarProducto([FromBody] AgregarItemCarritoRequest request)
        {
            try
            {
                var carrito = await _carritoServicio.AgregarProductoAsync(request);
                return Ok(carrito);
            }
            catch (StockInsuficienteException ex)
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

       
        [HttpPut("{clienteId:int}/items/{itemId:int}")]
        public async Task<ActionResult<CarritoDto>> ModificarCantidad(
            int clienteId, int itemId, [FromBody] ModificarCantidadRequest request)
        {
            try
            {
                var carrito = await _carritoServicio.ModificarCantidadAsync(clienteId, itemId, request.Cantidad);
                return Ok(carrito);
            }
            catch (StockInsuficienteException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

       
        [HttpDelete("{clienteId:int}/items/{itemId:int}")]
        public async Task<ActionResult<CarritoDto>> EliminarProducto(int clienteId, int itemId)
        {
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
                return Forbid(ex.Message);
            }
        }

        
        [HttpDelete("{clienteId:int}")]
        public async Task<ActionResult<CarritoDto>> VaciarCarrito(int clienteId)
        {
            var carrito = await _carritoServicio.VaciarCarritoAsync(clienteId);
            return Ok(carrito);
        }
    }
}
