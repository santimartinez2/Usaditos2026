using Microsoft.AspNetCore.Identity;
using Usaditos2026.BD.Datos.Entity;
using Usaditos2026.Repositorio.Interfaces;
using Usaditos2026.Servicios.Interfaces;
using Usaditos2026.Shared.DTOs;

namespace Usaditos2026.Servicios
{
    public class AuthServicio : IAuthServicio
    {
        private readonly IClienteRepositorio _clienteRepositorio;
        private readonly ICarritoServicio _carritoServicio;
        private readonly PasswordHasher<Cliente> _hasher = new();

        public AuthServicio(IClienteRepositorio clienteRepositorio, ICarritoServicio carritoServicio)
        {
            _clienteRepositorio = clienteRepositorio;
            _carritoServicio = carritoServicio;
        }

        public async Task<ClienteDto> RegistrarAsync(RegistroRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre))
                throw new ArgumentException("El nombre es obligatorio.");

            var email = NormalizarEmail(request.Email);

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("El email es obligatorio.");

            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
                throw new ArgumentException("La contraseña debe tener al menos 6 caracteres.");

            if (await _clienteRepositorio.ObtenerPorEmailAsync(email) is not null)
                throw new InvalidOperationException("Ya existe un cliente registrado con ese email.");

            var cliente = new Cliente
            {
                Nombre = request.Nombre.Trim(),
                Email = email,
                FechaRegistro = DateTime.UtcNow,
                Activo = true
            };

            cliente.PasswordHash = _hasher.HashPassword(cliente, request.Password);

            await _clienteRepositorio.AgregarAsync(cliente);
            await _clienteRepositorio.GuardarCambiosAsync();

            return MapearAClienteDto(cliente);
        }

        public async Task<ClienteDto?> IniciarSesionAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return null;

            var emailNormalizado = NormalizarEmail(email);
            var cliente = await _clienteRepositorio.ObtenerPorEmailAsync(emailNormalizado);

            if (cliente is null || !cliente.Activo)
                return null;

            var resultado = _hasher.VerifyHashedPassword(cliente, cliente.PasswordHash, password);

            if (resultado == PasswordVerificationResult.Failed)
                return null;

            if (resultado == PasswordVerificationResult.SuccessRehashNeeded)
            {
                cliente.PasswordHash = _hasher.HashPassword(cliente, password);
                await _clienteRepositorio.GuardarCambiosAsync();
            }

            await _carritoServicio.VaciarCarritoAsync(cliente.Id);

            return MapearAClienteDto(cliente);
        }

        public async Task CerrarSesionAsync(int clienteId)
        {
            await _carritoServicio.VaciarCarritoAsync(clienteId);
        }

        private static string NormalizarEmail(string email)
        {
            return (email ?? string.Empty).Trim().ToLowerInvariant();
        }

        private static ClienteDto MapearAClienteDto(Cliente cliente)
        {
            return new ClienteDto
            {
                Id = cliente.Id,
                Nombre = cliente.Nombre,
                Email = cliente.Email
            };
        }
    }
}
