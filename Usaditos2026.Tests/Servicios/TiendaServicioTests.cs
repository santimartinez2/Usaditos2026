using Microsoft.Extensions.Options;
using Usaditos2026.Servicios;
using Usaditos2026.Shared.DTOs;

namespace Usaditos2026.Tests.Servicios
{
    public class TiendaServicioTests
    {
        [Fact]
        public void ObtenerInfo_DevuelveValoresConfiguradosCorrectamente()
        {
            // Arrange
            var opciones = new TiendaOpciones
            {
                Direccion = "Av. San Martín 150, Hernando, Córdoba",
                Horario = "Lunes a viernes 9:00-18:00",
                Alias = "USADITOS.HERNANDO.MP",
                Titular = "Carolina Usaditos",
                WhatsApp = "+54 9 353 4123456",
                Telefono = "0353 4123456",
                Email = "contacto@usaditos.com",
                ZonaHoraria = "America/Argentina/Cordoba"
            };

            var servicio = new TiendaServicio(Options.Create(opciones));

            // Act
            var info = servicio.ObtenerInfo();

            // Assert
            Assert.Equal("Av. San Martín 150, Hernando, Córdoba", info.Direccion);
            Assert.Equal("Lunes a viernes 9:00-18:00", info.Horario);
            Assert.Equal("USADITOS.HERNANDO.MP", info.Alias);
            Assert.Equal("USADITOS.HERNANDO.MP", info.AliasTransferencia);
            Assert.Equal("Carolina Usaditos", info.Titular);
            Assert.Equal("Carolina Usaditos", info.TitularTransferencia);
            Assert.Equal("+54 9 353 4123456", info.WhatsApp);
            Assert.Equal("0353 4123456", info.Telefono);
            Assert.Equal("contacto@usaditos.com", info.Email);
            Assert.Equal("America/Argentina/Cordoba", info.ZonaHoraria);
        }

        [Fact]
        public void TiendaOpciones_Fallback_AliasTransferenciaYTitularTransferencia()
        {
            // Arrange: Cuando se configura solo AliasTransferencia y TitularTransferencia (como en versiones previas)
            var opciones = new TiendaOpciones
            {
                AliasTransferencia = "ALIAS.VIEJO",
                TitularTransferencia = "Titular Viejo"
            };

            // Assert: Las propiedades Alias y Titular resuelven automáticamente el valor
            Assert.Equal("ALIAS.VIEJO", opciones.Alias);
            Assert.Equal("ALIAS.VIEJO", opciones.AliasTransferencia);
            Assert.Equal("Titular Viejo", opciones.Titular);
            Assert.Equal("Titular Viejo", opciones.TitularTransferencia);
        }

        [Fact]
        public void TiendaOpciones_Fallback_AliasYTitularHaciaTransferencia()
        {
            // Arrange: Cuando se configuran variables de entorno directas Tienda__Alias y Tienda__Titular
            var opciones = new TiendaOpciones
            {
                Alias = "ALIAS.NUEVO",
                Titular = "Titular Nuevo"
            };

            // Assert: Las propiedades AliasTransferencia y TitularTransferencia resuelven el valor configurado
            Assert.Equal("ALIAS.NUEVO", opciones.Alias);
            Assert.Equal("ALIAS.NUEVO", opciones.AliasTransferencia);
            Assert.Equal("Titular Nuevo", opciones.Titular);
            Assert.Equal("Titular Nuevo", opciones.TitularTransferencia);
        }
    }
}
