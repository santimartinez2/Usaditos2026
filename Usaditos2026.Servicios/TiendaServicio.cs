using Microsoft.Extensions.Options;
using Usaditos2026.Servicios.Interfaces;
using Usaditos2026.Shared.DTOs;

namespace Usaditos2026.Servicios
{
    public class TiendaServicio : ITiendaServicio
    {
        private readonly TiendaInfoDto _info;

        public TiendaServicio(IOptions<TiendaInfoDto> options)
        {
            _info = options.Value;
        }

        public TiendaInfoDto ObtenerInfo()
        {
            return new TiendaInfoDto
            {
                Direccion = _info.Direccion ?? string.Empty,
                Horario = _info.Horario ?? string.Empty,
                AliasTransferencia = _info.AliasTransferencia ?? string.Empty,
                TitularTransferencia = _info.TitularTransferencia ?? string.Empty
            };
        }
    }
}
