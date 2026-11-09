using Usaditos2026.BD.Datos.Entity;

namespace Usaditos2026.Repositorio.Interfaces
{
    public interface ICatalogoRepositorio
    {
        Task<List<Producto>> ListarProductosActivosAsync();
    }
}
