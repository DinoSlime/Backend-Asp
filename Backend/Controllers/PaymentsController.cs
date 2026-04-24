using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Text.RegularExpressions;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // ⚠️ QUAN TRỌNG: Không đặt [Authorize] ở đây vì sẽ chặn luôn SePay
    public class PaymentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        // Tiêm DbContext vào để có thể tương tác với Database (Cập nhật trạng thái đơn)
        public PaymentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. API TẠO MÃ QR (Chỉ người dùng đã đăng nhập mới được gọi)
        [HttpPost("vietqr")]
        [Authorize]
        public IActionResult GetVietQR([FromBody] OrderResponse order)
        {
            try
            {
                string bankId = "MB";
                string accountNo = "56468877180054";
                string accountName = "THANH NINH BINH";

                // ⚠️ Sửa lại prefix thành "DH" (Ví dụ: DH105) để khớp với thuật toán lấy mã bên dưới
                string description = $"DH{order.Id}";

                string encodedAccountName = Uri.EscapeDataString(accountName);
                string encodedDescription = Uri.EscapeDataString(description);

                string qrUrl = $"https://img.vietqr.io/image/{bankId}-{accountNo}-compact.png" +
                               $"?amount={order.TotalPrice}" +
                               $"&addInfo={encodedDescription}" +
                               $"&accountName={encodedAccountName}";

                var response = new PaymentResponse
                {
                    QrCodeUrl = qrUrl,
                    OrderId = order.Id.ToString(),
                    TotalAmount = order.TotalPrice,
                    Description = description
                };

                return Ok(response);
            }
            catch (Exception e)
            {
                return BadRequest(new { message = "Lỗi tạo mã QR", error = e.Message });
            }
        }

        // 2. API NHẬN THÔNG BÁO TỪ SEPAY (Phải mở công khai để SePay gọi vào)
        [HttpPost("sepay-webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> SePayWebhook([FromBody] SePayWebhookRequest request)
        {
            try
            {
                // 1. Chỉ xử lý khi có tiền CỘNG VÀO tài khoản ("in")
                if (request.transferType != "in")
                    return Ok(new { success = true, message = "Bỏ qua giao dịch trừ tiền" });

                // 2. Lấy nội dung khách ghi khi chuyển khoản
                string description = request.content;

                // 3. Dùng Regex để "móc" lấy mã đơn hàng (Bắt các chữ số sau chữ "DH")
                var match = Regex.Match(description, @"DH(\d+)");
                if (!match.Success)
                    return Ok(new { success = true, message = "Không tìm thấy mã đơn hàng (DH...)" });

                long orderId = long.Parse(match.Groups[1].Value);

                // 4. Tìm đơn hàng trong Database
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                    return Ok(new { success = true, message = "Đơn hàng không tồn tại trên hệ thống" });

                // 5. Kiểm tra nếu số dư SePay báo về >= Tổng tiền trong DB
                if (request.transferAmount >= order.TotalMoney)
                {
                    order.Status = "PAID"; // Đổi thành ĐÃ THANH TOÁN
                    await _context.SaveChangesAsync();

                    return Ok(new { success = true, message = "Cập nhật thanh toán thành công!" });
                }
                else
                {
                    order.Status = "PARTIAL_PAID"; // Chuyển thiếu tiền
                    await _context.SaveChangesAsync();
                    return Ok(new { success = true, message = "Khách chuyển thiếu tiền" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }

    // Class hứng dữ liệu webhook từ SePay bắn về
    public class SePayWebhookRequest
    {
        public long id { get; set; }
        public string gateway { get; set; }
        public string transactionDate { get; set; }
        public string accountNumber { get; set; }
        public string content { get; set; } // Nội dung chuyển khoản
        public string transferType { get; set; } // "in" là tiền vào, "out" là tiền ra
        public decimal transferAmount { get; set; } // Số tiền chuyển
        public decimal accumulated { get; set; }
        public string referenceCode { get; set; }
    }
}