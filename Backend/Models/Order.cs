using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization; // 👇 BẮT BUỘC THÊM THƯ VIỆN NÀY ĐỂ DÙNG [JsonIgnore]

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
        public string? Email { get; set; } // Thêm ? vì khách hàng có thể không nhập Email

        [Column("phone_number")]
        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        [Column("address")]
        [Required]
        [MaxLength(255)]
        public string Address { get; set; }

        [Column("note")]
        [MaxLength(255)]
        public string? Note { get; set; } // Thêm ? vì ghi chú là tùy chọn

        [Column("order_date")]
        public DateTime OrderDate { get; set; }

        [Column("status")]
        public string? Status { get; set; } // Thêm ? để tránh lỗi khi mới tạo chưa set status

        [Column("total_money")]
        public decimal TotalMoney { get; set; }

        [Column("payment_method")]
        public string PaymentMethod { get; set; }

        // 👇 Mối quan hệ Many-To-One với User
        [Column("user_id")]
        public long? UserId { get; set; }

        [ForeignKey("UserId")]
        [JsonIgnore] // 👇 QUAN TRỌNG NHẤT: Tránh lỗi vòng lặp và không bắt React phải gửi cục User
        public User? User { get; set; } // Thêm dấu ?

        // Mối quan hệ One-To-Many với chi tiết đơn hàng (Giữ nguyên để nhận danh sách từ React)
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}