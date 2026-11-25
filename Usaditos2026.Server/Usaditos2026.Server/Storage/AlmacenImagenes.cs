using Microsoft.Extensions.Options;
using Usaditos2026.Servicios.Interfaces;
using Usaditos2026.Shared.Constantes;
using Usaditos2026.Shared.DTOs;

namespace Usaditos2026.Server.Storage
{
    public class AlmacenImagenes : IAlmacenImagenes
    {
        private readonly IWebHostEnvironment _entorno;
        private readonly ImagenesOpciones _opciones;

        public AlmacenImagenes(IWebHostEnvironment entorno, IOptions<ImagenesOpciones> opciones)
        {
            _entorno = entorno;
            _opciones = opciones.Value;
        }

        public async Task<string> GuardarAsync(Stream contenido, string nombreArchivo)
        {
            var nombre = Path.GetFileName(nombreArchivo);
            if (string.IsNullOrWhiteSpace(nombre) || nombre.Contains(".."))
                throw new ArgumentException("Nombre de archivo inválido.");

            var directorio = RutaAbsoluta();
            Directory.CreateDirectory(directorio);

            contenido.Position = 0;
            await using var archivo = new FileStream(
                Path.Combine(directorio, nombre), FileMode.CreateNew, FileAccess.Write, FileShare.None);
            await contenido.CopyToAsync(archivo);

            return $"{ImagenesConstantes.RutaPublica}/{nombre}";
        }

        public Task BorrarAsync(string? rutaRelativa)
        {
            if (string.IsNullOrWhiteSpace(rutaRelativa))
                return Task.CompletedTask;

            // Solo se pueden borrar rutas generadas por el servidor, nunca una ruta arbitraria del cliente.
            var prefijo = ImagenesConstantes.RutaPublica + "/";
            if (!rutaRelativa.StartsWith(prefijo, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Ruta de imagen inválida.");

            var nombre = Path.GetFileName(rutaRelativa);
            if (string.IsNullOrWhiteSpace(nombre) || nombre.Contains(".."))
                throw new ArgumentException("Ruta de imagen inválida.");

            var ruta = Path.Combine(RutaAbsoluta(), nombre);
            if (File.Exists(ruta))
                File.Delete(ruta);

            return Task.CompletedTask;
        }

        private string RutaAbsoluta()
        {
            var ruta = _opciones.Ruta;
            return Path.IsPathRooted(ruta)
                ? ruta
                : Path.Combine(_entorno.ContentRootPath, ruta.Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
