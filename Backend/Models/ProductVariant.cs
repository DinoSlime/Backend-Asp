using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("product_variants")]
    public class ProductVariant
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("size")]
        public int Size { get; set; } // Dùng int thay cho Integer của Java

        [Column("color")]
        public string Color { get; set; }

        [Column("image_url")]
        public string ImageUrl { get; set; }

        [Column("stock")]
        public int Stock { get; set; }

        // 👇 Mối quan hệ Many-To-One quay ngược lại Product mẹ
        [Column("product_id")]
        public long ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; } // Navigation property
    }
}