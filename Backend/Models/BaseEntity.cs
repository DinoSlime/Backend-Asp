using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    // abstract tương đương với @MappedSuperclass bên Java
    public abstract class BaseEntity
    {
        // [Key] đánh dấu khóa chính (@Id)
        // [DatabaseGenerated] tương đương @GeneratedValue(strategy = GenerationType.IDENTITY)
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; } // Dùng long tương đương Long của Java

        // Không cần Lombok (@Getter, @Setter) vì C# có sẵn { get; set; }
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; } // Thêm dấu ? để cho phép null khi mới tạo
    }
}