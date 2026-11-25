using System.IO;

namespace Usaditos2026.Servicios.Interfaces
{
    public interface IAlmacenImagenes
    {
        Task<string> GuardarAsync(Stream contenido, string nombreArchivo);
        Task BorrarAsync(string? rutaRelativa);
    }
}
