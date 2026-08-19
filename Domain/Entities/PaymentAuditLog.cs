using System;

namespace Domain.Entities
{
    public class PaymentAuditLog
    {
        public string Id { get; set; } = string.Empty;
        public string MessageId { get; set; } = string.Empty;
        public int UserId { get; set; }
        public int GameId { get; set; }
        public double Price { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime ProcessadoEm { get; set; } = DateTime.UtcNow;
    }
}
