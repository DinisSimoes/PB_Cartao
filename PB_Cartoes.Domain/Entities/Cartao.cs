namespace PB_Cartoes.Domain.Entities
{
    public class Cartao
    {
        public Guid Id { get; set; }
        public Guid ClienteId { get; set; }
        public string NumeroCartao { get; set; } = string.Empty;
        public decimal Limite { get; set; }
        public DateTime CriadoEmUtc { get; set; }
    }
}