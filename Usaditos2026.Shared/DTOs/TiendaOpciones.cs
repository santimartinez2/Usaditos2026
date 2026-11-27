namespace Usaditos2026.Shared.DTOs
{
    public class TiendaOpciones
    {
        public const string Seccion = "Tienda";

        public string Direccion { get; set; } = string.Empty;
        public string Horario { get; set; } = string.Empty;

        private string? _alias;
        public string Alias
        {
            get => !string.IsNullOrWhiteSpace(_alias) ? _alias : (_aliasTransferencia ?? string.Empty);
            set => _alias = value;
        }

        private string? _aliasTransferencia;
        public string AliasTransferencia
        {
            get => !string.IsNullOrWhiteSpace(_aliasTransferencia) ? _aliasTransferencia : (_alias ?? string.Empty);
            set => _aliasTransferencia = value;
        }

        private string? _titular;
        public string Titular
        {
            get => !string.IsNullOrWhiteSpace(_titular) ? _titular : (_titularTransferencia ?? string.Empty);
            set => _titular = value;
        }

        private string? _titularTransferencia;
        public string TitularTransferencia
        {
            get => !string.IsNullOrWhiteSpace(_titularTransferencia) ? _titularTransferencia : (_titular ?? string.Empty);
            set => _titularTransferencia = value;
        }

        public string WhatsApp { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ZonaHoraria { get; set; } = "America/Argentina/Cordoba";
    }
}
