using Microsoft.Extensions.Options;
using Usaditos2026.Servicios.Interfaces;
using Usaditos2026.Shared.DTOs;

namespace Usaditos2026.Servicios
{
    public class TiendaServicio : ITiendaServicio
    {
        private readonly TiendaOpciones _info;

        public TiendaServicio(IOptions<TiendaOpciones> options)
        {
            _info = options.Value;
        }

        public TiendaInfoDto ObtenerInfo()
        {
            return new TiendaInfoDto
            {
                Direccion = _info.Direccion ?? string.Empty,
                Horario = _info.Horario ?? string.Empty,
                Alias = _info.Alias ?? string.Empty,
                AliasTransferencia = _info.AliasTransferencia ?? string.Empty,
                Titular = _info.Titular ?? string.Empty,
                TitularTransferencia = _info.TitularTransferencia ?? string.Empty,
                WhatsApp = _info.WhatsApp ?? string.Empty,
                Telefono = _info.Telefono ?? string.Empty,
                Email = _info.Email ?? string.Empty,
                ZonaHoraria = _info.ZonaHoraria ?? string.Empty
            };
        }
    }
}
