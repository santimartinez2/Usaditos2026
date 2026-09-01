using Microsoft.AspNetCore.Mvc;
using Usaditos2026.Servicios.Interfaces;
using Usaditos2026.Shared.DTOs;
using Usaditos2026.Shared.Excepciones;

namespace Usaditos2026.Server.Controllers
{
    // Caso de uso: Agregar y eliminar producto del carrito.
    [ApiController]
    [Route("api/[controller]")]
    public class CarritoController : ControllerBase
    {
        private readonly ICarritoServicio _carritoServicio;

        public CarritoController(ICarritoServicio carritoServicio)
        {
            _carritoServicio = carritoServicio;
        }

        // GET api/carrito/{clienteId}
        // 6. El cliente accede al carrito y visualiza el detalle de los productos seleccionados.
        [HttpGet("{clienteId:int}")]
        public async Task<ActionResult<CarritoDto>> ObtenerCarrito(int clienteId)
        {
            var carrito = await _carritoServicio.ObtenerCarritoAsync(clienteId);
            return Ok(carrito);
        }

        // POST api/carrito/items
        // 2-4. El cliente selecciona un producto y el sistema lo agrega si hay stock.
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
                // 3a. El sistema notifica al cliente que el producto no está disponible.
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

        // PUT api/carrito/{clienteId}/items/{itemId}
        // 5a. El cliente modifica la cantidad de un producto ya agregado.
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

        // DELETE api/carrito/{clienteId}/items/{itemId}
        // 7-8. El cliente elimina un producto del carrito y el sistema recalcula el total.
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

        // DELETE api/carrito/{clienteId}
        // 7a. El cliente decide vaciar todo el carrito.
        [HttpDelete("{clienteId:int}")]
        public async Task<ActionResult<CarritoDto>> VaciarCarrito(int clienteId)
        {
            var carrito = await _carritoServicio.VaciarCarritoAsync(clienteId);
            return Ok(carrito);
        }
    }
}
