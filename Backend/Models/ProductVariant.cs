using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization; // Thêm thư viện này để dùng JsonIgnore

namespace Backend.Models
{
    [Table("product_variants")]
    public class ProductVariant
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("size")]
        public int Size { get; set; }

        [Column("color")]
        public string Color { get; set; }

        [Column("image_url")]
        // 👇 Thêm dấu ? vì trong Payload của bạn, ImageUrl đôi khi để trống
        public string? ImageUrl { get; set; }

        [Column("stock")]
        public int Stock { get; set; }

        // 👇 Mối quan hệ Many-To-One quay ngược lại Product mẹ
        [Column("product_id")]
        public long ProductId { get; set; }

        [ForeignKey("ProductId")]
        // 👇 THÊM DẤU ? VÀ JSON IGNORE: 
        // 1. Để không bắt buộc gửi nguyên cục Product từ React lên.
        // 2. Để tránh lỗi vòng lặp JSON khi trả dữ liệu về.
        [JsonIgnore]
        public Product? Product { get; set; }
    }
}