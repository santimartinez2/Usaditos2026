using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Usaditos2026.Shared.DTOs;

namespace Usaditos2026.Server.Security
{
    public static class AuthCookieHelper
    {
        public static async Task IniciarSesionAsync(HttpContext httpContext, ClienteDto cliente)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, cliente.Id.ToString()),
                new(ClaimTypes.Name, cliente.Nombre),
                new(ClaimTypes.Email, cliente.Email)
            };

            var identidad = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identidad));
        }

        public static Task CerrarSesionAsync(HttpContext httpContext)
        {
            return httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
