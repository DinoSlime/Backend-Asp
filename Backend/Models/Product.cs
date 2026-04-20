using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("products")]
    public class Product : BaseEntity
    {
        [Column("name")]
        [Required]
        [MaxLength(350)]
        public string Name { get; set; }

        [Column("price")]
        public decimal Price { get; set; } // Tiếp tục dùng decimal thay vì Float cho tiền tệ

        [Column("thumbnail")]
        public string Thumbnail { get; set; }

        // 👇 Xử lý kiểu TEXT cho SQL Server
        [Column("description", TypeName = "nvarchar(max)")]
        public string Description { get; set; }

        // 👇 Mối quan hệ Many-To-One với Category
        [Column("category_id")]
        public long CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category Category { get; set; } // Navigation property

        // 👇 Mối quan hệ One-To-Many với ProductVariant
        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    }
}