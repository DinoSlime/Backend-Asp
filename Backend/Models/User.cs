using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization; // 💡 Thư viện này chứa [JsonIgnore]
using Microsoft.EntityFrameworkCore;

namespace Backend.Models
{
    [Table("users")]
    [Index(nameof(Username), IsUnique = true)] // Tương đương unique = true của Username
    public class User : BaseEntity
    {
        [Column("fullname")]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Column("username")]
        [Required]
        [MaxLength(100)]
        public string Username { get; set; }

        [Column("phone_number")]
        [Required]
        [MaxLength(10)]
        public string PhoneNumber { get; set; }

        [Column("address")]
        [MaxLength(200)]
        public string Address { get; set; }

        [Column("password")]
        [Required]
        [MaxLength(200)]
        [JsonIgnore] // Chặn không cho EF Core trả Password về frontend (Bảo mật)
        public string Password { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("date_of_birth")]
        public DateTime? DateOfBirth { get; set; } // Dùng DateTime? thay cho Date của Java

        [Column("facebook_account_id")]
        public int FacebookAccountId { get; set; } = 0;

        [Column("google_account_id")]
        public int GoogleAccountId { get; set; } = 0;

        // 👇 Mối quan hệ Many-To-One với Role
        [Column("role_id")]
        public long RoleId { get; set; }

        [ForeignKey("RoleId")]
        public Role Role { get; set; }
    }
}