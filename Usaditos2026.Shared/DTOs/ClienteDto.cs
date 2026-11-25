using Usaditos2026.Shared.Constantes;

namespace Usaditos2026.Shared.DTOs
{
    public class ClienteDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = RolConstantes.Cliente;
    }
}
