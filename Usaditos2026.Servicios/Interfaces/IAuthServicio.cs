using Usaditos2026.Shared.DTOs;

namespace Usaditos2026.Servicios.Interfaces
{
    public interface IAuthServicio
    {
        Task<ClienteDto> RegistrarAsync(RegistroRequest request);
        Task<ClienteDto?> IniciarSesionAsync(string email, string password);
        Task CerrarSesionAsync(int clienteId);
    }
}
