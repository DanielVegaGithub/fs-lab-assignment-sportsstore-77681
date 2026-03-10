namespace SportsStore.Models
{
    public class PaymentResult
    {
        public bool Succeeded { get; set; }
        public bool Cancelled { get; set; }
        public string Status { get; set; } = "Pending";
        public string? PaymentIntentId { get; set; }
        public string? ConfirmationId { get; set; }
        public decimal Amount { get; set; }
        public string? Currency { get; set; }
        public string? ErrorMessage { get; set; }
    }
}