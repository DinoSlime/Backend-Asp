using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("products")]
    public class Product : BaseEntity
    {
        [Column("name")]
        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        [MaxLength(350)]
        public string Name { get; set; } // Bắt buộc phải có tên

        [Column("price")]
        public decimal Price { get; set; } // Bắt buộc phải có giá

        [Column("thumbnail")]
        // 👇 Thêm dấu ? để cho phép rỗng khi chưa upload ảnh
        public string? Thumbnail { get; set; }

        [Column("description", TypeName = "nvarchar(max)")]
        // 👇 Thêm dấu ? để cho phép rỗng
        public string? Description { get; set; }

        [Column("category_id")]
        public long CategoryId { get; set; } // Chỉ cần ID là đủ

        [ForeignKey("CategoryId")]
        // 👇 ĐÂY LÀ DÒNG CHỮA LỖI: Thêm dấu ? để .NET không đòi cả Object Category nữa
        public Category? Category { get; set; }

        // 👇 Mối quan hệ One-To-Many với ProductVariant
        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    }
}