using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization; // 👇 BẮT BUỘC THÊM ĐỂ DÙNG JsonIgnore và JsonPropertyName

namespace Backend.Models
{
    [Table("order_details")]
    public class OrderDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        // 👇 1. Mối quan hệ với Order (Đơn hàng)
        [Column("order_id")]
        public long OrderId { get; set; }

        [ForeignKey("OrderId")]
        [JsonIgnore] // Tránh đệ quy vô tận
        public Order? Order { get; set; } // Thêm ? cho phép null từ request

        // 👇 2. Mối quan hệ với Product (Sản phẩm gốc)
        [Column("product_id")]
        public long ProductId { get; set; }

        [ForeignKey("ProductId")]
       
        public Product? Product { get; set; }

        // 👇 3. Mối quan hệ với ProductVariant (Phân loại Size/Màu)
        [Column("variant_id")]
        public long VariantId { get; set; }

        [ForeignKey("VariantId")]
       
        public ProductVariant? Variant { get; set; }

        // 👇 4. Các trường dữ liệu tính toán
        [Column("price")]
        public decimal Price { get; set; }

        [Column("number_of_products")]
        [JsonPropertyName("quantity")] // 👇 Ép C# hiểu chữ "quantity" từ React gửi lên
        public int NumberOfProducts { get; set; }

        [Column("total_money")]
        public decimal TotalMoney { get; set; }
    }
}