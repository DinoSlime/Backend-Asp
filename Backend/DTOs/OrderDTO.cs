namespace Backend.DTOs
{
    public class OrderDTO
    {
        public int? UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Note { get; set; }
        public decimal TotalMoney { get; set; } // Dùng decimal cho tiền
        public string PaymentMethod { get; set; } = string.Empty;

        public List<OrderDetailDTO> OrderDetails { get; set; } = new List<OrderDetailDTO>();
    }

    public class OrderDetailDTO
    {
        public int ProductId { get; set; }
        public int VariantId { get; set; } // Chứa size/color
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}