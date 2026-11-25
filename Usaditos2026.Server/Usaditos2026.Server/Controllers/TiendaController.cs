using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Usaditos2026.Servicios.Interfaces;
using Usaditos2026.Shared.DTOs;

namespace Usaditos2026.Server.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/tienda")]
    public class TiendaController : ControllerBase
    {
        private readonly ITiendaServicio _tiendaServicio;

        public TiendaController(ITiendaServicio tiendaServicio)
        {
            _tiendaServicio = tiendaServicio;
        }

        [HttpGet]
        public ActionResult<TiendaInfoDto> Obtener()
        {
            return Ok(_tiendaServicio.ObtenerInfo());
        }
    }
}
