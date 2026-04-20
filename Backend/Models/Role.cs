using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore; // Cần thư viện này để xài tính năng [Index]

namespace Backend.Models
{
    [Table("roles")]
    // 👇 Cách C# (EF Core) xử lý unique = true
    [Index(nameof(Name), IsUnique = true)]
    public class Role
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("name")]
        [Required]        // Tương đương nullable = false
        [MaxLength(50)]   // 💡 Bổ sung thêm giới hạn ký tự (Khuyên dùng)
        public string Name { get; set; }
    }
}