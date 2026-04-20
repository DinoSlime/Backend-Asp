using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("orders")]
    public class Order : BaseEntity
    {
        [Column("fullname")]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Column("email")]
        [MaxLength(100)]
        public string Email { get; set; }

        [Column("phone_number")]
        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        [Column("address")]
        [Required]
        [MaxLength(255)] // Tăng lên 255 theo ý bạn
        public string Address { get; set; }

        [Column("note")]
        [MaxLength(255)]
        public string Note { get; set; }

        [Column("order_date")]
        public DateTime OrderDate { get; set; } // DateTime của C# thay cho LocalDateTime

        [Column("status")]
        public string Status { get; set; } // PENDING, SHIPPING, DELIVERED, CANCELLED

        [Column("total_money")]
        public decimal TotalMoney { get; set; } // 💡 C# chuộng dùng decimal cho tiền tệ thay vì float

        [Column("payment_method")]
        public string PaymentMethod { get; set; } // COD, BANK

        // 👇 QUAN TRỌNG 1: Mối quan hệ Many-To-One với User
        [Column("user_id")]
        public long? UserId { get; set; } // Dấu ? biểu thị cho phép Null (khách vãng lai)

        [ForeignKey("UserId")]
        public User User { get; set; } // Navigation property (Thuộc tính điều hướng)

        // 👇 QUAN TRỌNG 2: Mối quan hệ One-To-Many với chi tiết đơn hàng
        // Trong C#, thường dùng ICollection thay vì List cho các mối quan hệ
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}