using Backend.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Web;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        [HttpPost("vietqr")]
        public IActionResult GetVietQR([FromBody] OrderResponse order)
        {
            try
            {
                // 1. Cấu hình thông tin ngân hàng của bạn
                string bankId = "MB";
                string accountNo = "56468877180054";
                string accountName = "THANH NINH BINH";
                string description = $"THANHTOAN {order.Id}";

                // 2. Encode các tham số để tránh lỗi ký tự đặc biệt trong URL (như khoảng trắng)
                string encodedAccountName = Uri.EscapeDataString(accountName);
                string encodedDescription = Uri.EscapeDataString(description);

                // 3. Cấu trúc link VietQR (Sử dụng string interpolation $"" của C# cho gọn)
                string qrUrl = $"https://img.vietqr.io/image/{bankId}-{accountNo}-compact.png" +
                               $"?amount={order.TotalPrice}" +
                               $"&addInfo={encodedDescription}" +
                               $"&accountName={encodedAccountName}";

                // 4. Tạo response trả về giống hệt cấu trúc Java cũ
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
    }
}