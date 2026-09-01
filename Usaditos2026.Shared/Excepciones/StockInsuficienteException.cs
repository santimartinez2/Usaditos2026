namespace Usaditos2026.Shared.Excepciones
{
    public class StockInsuficienteException : Exception
    {
        public StockInsuficienteException(string mensaje) : base(mensaje)
        {
        }
    }
}
