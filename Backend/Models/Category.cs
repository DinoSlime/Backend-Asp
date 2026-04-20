using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("categories")]
    public class Category : BaseEntity
    {
        [Column("name")]
        [Required]        // Tương đương nullable = false bên Java
        [MaxLength(100)]  // Tương đương length = 100 bên Java
        public string Name { get; set; }
    }
}