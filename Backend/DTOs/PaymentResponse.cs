namespace Backend.DTOs
{
    public class PaymentResponse
    {
        public string QrCodeUrl { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public long TotalAmount { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}