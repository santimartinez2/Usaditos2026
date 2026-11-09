using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Usaditos2026.Server.Security;
using Usaditos2026.Servicios.Interfaces;
using Usaditos2026.Shared.DTOs;

namespace Usaditos2026.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthServicio _authServicio;

        public AuthController(IAuthServicio authServicio)
        {
            _authServicio = authServicio;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var cliente = await _authServicio.IniciarSesionAsync(request.Email, request.Password);

            if (cliente is null)
                return Unauthorized(new { mensaje = "Email o contraseña incorrectos." });

            await AuthCookieHelper.IniciarSesionAsync(HttpContext, cliente);

            return Ok(new { mensaje = "Sesión iniciada.", cliente });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var valor = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(valor, out var clienteId))
                await _authServicio.CerrarSesionAsync(clienteId);

            await AuthCookieHelper.CerrarSesionAsync(HttpContext);

            if (Request.HasFormContentType)
                return Redirect("/login");

            return Ok(new { mensaje = "Sesión cerrada." });
        }
    }
}
