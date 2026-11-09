using Microsoft.EntityFrameworkCore;
using Usaditos2026.BD.Datos;
using Usaditos2026.BD.Datos.Entity;
using Usaditos2026.Repositorio.Interfaces;

namespace Usaditos2026.Repositorio
{
    public class ClienteRepositorio : IClienteRepositorio
    {
        private readonly AppDbContext _context;

        public ClienteRepositorio(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Cliente?> ObtenerPorEmailAsync(string email)
        {
            return await _context.Clientes.FirstOrDefaultAsync(c => c.Email == email);
        }

        public async Task AgregarAsync(Cliente cliente)
        {
            await _context.Clientes.AddAsync(cliente);
        }

        public async Task GuardarCambiosAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
