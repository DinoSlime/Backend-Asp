using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
using Backend.DTOs; // Sử dụng DTO vừa tạo

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Tạo đơn hàng (POST: api/orders)
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] Order order)
        {
            try
            {
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                return Ok(order);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        // 2. Lấy đơn hàng theo User ID (GET: api/orders/user/5)
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetOrdersByUser(int userId)
        {
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .ToListAsync();
            return Ok(orders);
        }

        // 3. Lấy chi tiết 1 đơn hàng (GET: api/orders/5)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound("Không tìm thấy đơn hàng");
            return Ok(order);
        }

        // 4. Admin lấy tất cả đơn hàng (GET: api/orders/admin/get-all)
        [HttpGet("admin/get-all")]
        public async Task<IActionResult> GetAllOrders()
        {
            return Ok(await _context.Orders.ToListAsync());
        }

        // 5. Cập nhật trạng thái đơn (PUT: api/orders/admin/update-status/5?status=SHIPPING)
        [HttpPut("admin/update-status/{id}")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromQuery] string status)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order == null) return NotFound("Đơn hàng không tồn tại");

                order.Status = status; // Giả sử model Order của bạn có trường Status
                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật trạng thái thành công" });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}