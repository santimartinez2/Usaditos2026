using Usaditos2026.BD.Datos.Entity;

namespace Usaditos2026.Repositorio.Interfaces
{
    public interface IClienteRepositorio
    {
        Task<Cliente?> ObtenerPorEmailAsync(string email);
        Task AgregarAsync(Cliente cliente);
        Task GuardarCambiosAsync();
    }
}
