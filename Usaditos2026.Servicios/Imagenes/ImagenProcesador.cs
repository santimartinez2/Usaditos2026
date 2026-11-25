using System;
using System.IO;
using System.Threading.Tasks;
using SkiaSharp;
using Usaditos2026.Shared.Constantes;

namespace Usaditos2026.Servicios.Imagenes
{
    internal static class ImagenProcesador
    {
        private static readonly byte[] PrefijoJpeg = { 0xFF, 0xD8, 0xFF };
        private static readonly byte[] PrefijoPng = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        private static readonly byte[] PrefijoRiff = { 0x52, 0x49, 0x46, 0x46 };
        private static readonly byte[] PrefijoWebp = { 0x57, 0x45, 0x42, 0x50 };

        public static async Task<(byte[] Contenido, string Extension)> ProcesarAsync(Stream contenido)
        {
            var formato = DetectarFormato(contenido);

            contenido.Position = 0;
            using var codec = SKCodec.Create(contenido)
                ?? throw new ArgumentException("La imagen no se pudo leer. Subí un JPEG, PNG o WebP válido.");

            ValidarDimensiones(codec.Info);
            var origen = codec.EncodedOrigin;

            contenido.Position = 0;
            using var decodificada = SKBitmap.Decode(contenido)
                ?? throw new ArgumentException("La imagen no se pudo decodificar.");

            // La orientación EXIF se aplica antes de redimensionar y de re-codificar: los metadatos no se conservan.
            var (orientada, esTemporal) = AplicarOrientacion(decodificada, origen);
            SKBitmap? redimensionada = null;

            try
            {
                var escala = Math.Min(1f,
                    (float)ImagenesConstantes.LadoRedimensionado / Math.Max(orientada.Width, orientada.Height));
                var ancho = Math.Max(1, (int)Math.Round(orientada.Width * escala));
                var alto = Math.Max(1, (int)Math.Round(orientada.Height * escala));

                if (ancho != orientada.Width || alto != orientada.Height)
                {
                    var info = new SKImageInfo(ancho, alto, SKColorType.Rgba8888, SKAlphaType.Premul);
                    var muestreo = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear);
                    redimensionada = orientada.Resize(info, muestreo)
                        ?? throw new ArgumentException("No se pudo redimensionar la imagen.");
                }

                using var imagen = SKImage.FromBitmap(redimensionada ?? orientada);
                using var datos = imagen.Encode(ObtenerFormatoCodificacion(formato), ImagenesConstantes.Calidad)
                    ?? throw new ArgumentException("No se pudo codificar la imagen.");
                using var salida = datos.AsStream();
                using var memoria = new MemoryStream();
                await salida.CopyToAsync(memoria);

                return (memoria.ToArray(), ObtenerExtension(formato));
            }
            finally
            {
                redimensionada?.Dispose();
                if (esTemporal)
                    orientada.Dispose();
            }
        }

        private static FormatoImagen DetectarFormato(Stream contenido)
        {
            if (!contenido.CanSeek)
                throw new ArgumentException("El stream de la imagen debe permitir lectura desde el inicio.");

            contenido.Position = 0;
            var cabecera = new byte[12];
            var leidos = contenido.Read(cabecera, 0, cabecera.Length);
            contenido.Position = 0;

            if (Coincide(cabecera, leidos, PrefijoJpeg))
                return FormatoImagen.Jpeg;

            if (Coincide(cabecera, leidos, PrefijoPng))
                return FormatoImagen.Png;

            if (Coincide(cabecera, leidos, PrefijoRiff) && Coincide(cabecera.AsSpan(8), leidos - 8, PrefijoWebp))
                return FormatoImagen.Webp;

            throw new ArgumentException("Formato de imagen no admitido. Solo se aceptan JPEG, PNG y WebP.");
        }

        private static bool Coincide(ReadOnlySpan<byte> cabecera, int leidos, byte[] prefijo)
        {
            if (leidos < prefijo.Length || cabecera.Length < prefijo.Length)
                return false;

            for (var i = 0; i < prefijo.Length; i++)
            {
                if (cabecera[i] != prefijo[i])
                    return false;
            }

            return true;
        }

        private static void ValidarDimensiones(SKImageInfo info)
        {
            if (info.Width <= 0 || info.Height <= 0)
                throw new ArgumentException("Las dimensiones de la imagen no son válidas.");

            if (info.Width > ImagenesConstantes.LadoMaximoPixeles || info.Height > ImagenesConstantes.LadoMaximoPixeles)
                throw new ArgumentException(
                    $"La imagen es demasiado grande: el lado no puede superar los {ImagenesConstantes.LadoMaximoPixeles} píxeles.");

            var megapixeles = (long)info.Width * info.Height;
            if (megapixeles > ImagenesConstantes.MaximoMegapixeles * 1_000_000L)
                throw new ArgumentException(
                    $"La imagen es demasiado grande: no puede superar los {ImagenesConstantes.MaximoMegapixeles} megapíxeles.");
        }

        private static (SKBitmap Bitmap, bool EsTemporal) AplicarOrientacion(SKBitmap bitmapa, SKEncodedOrigin origen)
        {
            switch (origen)
            {
                case SKEncodedOrigin.TopRight:
                    return (VoltearHorizontal(bitmapa), true);
                case SKEncodedOrigin.BottomRight:
                    return (Rotar(bitmapa, 180), true);
                case SKEncodedOrigin.BottomLeft:
                    return (VoltearVertical(bitmapa), true);
                case SKEncodedOrigin.LeftTop:
                {
                    using var girada = Rotar(bitmapa, 90);
                    return (VoltearHorizontal(girada), true);
                }
                case SKEncodedOrigin.RightTop:
                    return (Rotar(bitmapa, 90), true);
                case SKEncodedOrigin.RightBottom:
                {
                    using var girada = Rotar(bitmapa, 270);
                    return (VoltearHorizontal(girada), true);
                }
                case SKEncodedOrigin.LeftBottom:
                    return (Rotar(bitmapa, 270), true);
                default:
                    return (bitmapa, false);
            }
        }

        private static SKBitmap Rotar(SKBitmap bitmapa, float grados)
        {
            var rotada = new SKBitmap(bitmapa.Height, bitmapa.Width, bitmapa.ColorType, bitmapa.AlphaType);

            using var lienzo = new SKCanvas(rotada);
            if (grados == 90)
                lienzo.Translate(bitmapa.Height, 0);
            else if (grados == 180)
                lienzo.Translate(bitmapa.Width, bitmapa.Height);
            else
                lienzo.Translate(0, bitmapa.Width);

            lienzo.RotateDegrees(grados);
            lienzo.DrawBitmap(bitmapa, 0, 0, SKSamplingOptions.Default);
            return rotada;
        }

        private static SKBitmap VoltearHorizontal(SKBitmap bitmapa)
        {
            var volteada = new SKBitmap(bitmapa.Width, bitmapa.Height, bitmapa.ColorType, bitmapa.AlphaType);
            using var lienzo = new SKCanvas(volteada);
            lienzo.Translate(bitmapa.Width, 0);
            lienzo.Scale(-1, 1);
            lienzo.DrawBitmap(bitmapa, 0, 0, SKSamplingOptions.Default);
            return volteada;
        }

        private static SKBitmap VoltearVertical(SKBitmap bitmapa)
        {
            var volteada = new SKBitmap(bitmapa.Width, bitmapa.Height, bitmapa.ColorType, bitmapa.AlphaType);
            using var lienzo = new SKCanvas(volteada);
            lienzo.Translate(0, bitmapa.Height);
            lienzo.Scale(1, -1);
            lienzo.DrawBitmap(bitmapa, 0, 0, SKSamplingOptions.Default);
            return volteada;
        }

        private static SKEncodedImageFormat ObtenerFormatoCodificacion(FormatoImagen formato)
        {
            return formato switch
            {
                FormatoImagen.Jpeg => SKEncodedImageFormat.Jpeg,
                FormatoImagen.Webp => SKEncodedImageFormat.Webp,
                _ => SKEncodedImageFormat.Png
            };
        }

        private static string ObtenerExtension(FormatoImagen formato)
        {
            return formato switch
            {
                FormatoImagen.Jpeg => "jpg",
                FormatoImagen.Webp => "webp",
                _ => "png"
            };
        }
    }
}
