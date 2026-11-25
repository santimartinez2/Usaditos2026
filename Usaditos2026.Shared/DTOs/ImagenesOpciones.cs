using Usaditos2026.Shared.Constantes;

namespace Usaditos2026.Shared.DTOs
{
    public class ImagenesOpciones
    {
        public string Ruta { get; set; } = "App_Data/uploads/productos";
        public int TamanoMaximoMb { get; set; } = ImagenesConstantes.TamanoMaximoPorDefectoMb;
    }
}
