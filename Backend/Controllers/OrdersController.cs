using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Toàn bộ Controller này phải đăng nhập mới được dùng
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Tạo đơn hàng (Bao gồm logic TRỪ KHO)
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderDTO orderDTO)
        {
            // Bắt đầu Transaction: Đảm bảo nếu lỗi ở bước cuối thì rollback lại toàn bộ
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Tạo Order (Bảng cha)
                var order = new Order
                {
                    UserId = orderDTO.UserId,
                    FullName = orderDTO.FullName,
                    PhoneNumber = orderDTO.PhoneNumber,
                    Address = orderDTO.Address,
                    Note = orderDTO.Note,
                    TotalMoney = orderDTO.TotalMoney,
                    PaymentMethod = orderDTO.PaymentMethod,
                    Status = "PENDING",
                    OrderDate = DateTime.Now
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync(); // Lưu để lấy Id của Order

                // 2. Xử lý từng món hàng và Trừ Kho
                var details = new List<OrderDetail>();

                foreach (var itemDTO in orderDTO.OrderDetails)
                {
                    // Lấy sản phẩm và biến thể
                    var product = await _context.Products.FindAsync(itemDTO.ProductId);
                    if (product == null) throw new Exception($"Không tìm thấy sản phẩm ID: {itemDTO.ProductId}");

                    var variant = await _context.ProductVariants.FindAsync(itemDTO.VariantId);
                    if (variant == null) throw new Exception($"Không tìm thấy biến thể ID: {itemDTO.VariantId}");

                    // CHECK TỒN KHO & TRỪ KHO
                    if (variant.Stock < itemDTO.Quantity)
                    {
                        throw new Exception($"Sản phẩm {product.Name} (Size: {variant.Size}) không đủ hàng! Còn lại: {variant.Stock}");
                    }

                    variant.Stock -= itemDTO.Quantity; // Trừ kho
                    _context.ProductVariants.Update(variant);

                    // Tạo OrderDetail (Bảng con)
                    var detail = new OrderDetail
                    {
                        OrderId = order.Id,
                        ProductId = product.Id,
                        VariantId = variant.Id,
                        Price = itemDTO.Price,
                        NumberOfProducts = itemDTO.Quantity,
                        TotalMoney = itemDTO.Price * itemDTO.Quantity
                    };
                    details.Add(detail);
                }

                // Lưu danh sách chi tiết đơn hàng
                _context.OrderDetails.AddRange(details);
                await _context.SaveChangesAsync();

                // Xác nhận thành công toàn bộ quá trình
                await transaction.CommitAsync();

                return Ok(order);
            }
            catch (Exception e)
            {
                // Nếu có bất kỳ lỗi gì (vd: hết hàng), hủy bỏ toàn bộ thay đổi
                await transaction.RollbackAsync();
                return BadRequest(e.Message);
            }
        }

        // 2. Lấy đơn hàng theo User ID
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetOrdersByUser(int userId)
        {
            var orders = await _context.Orders
                .Include(o => o.OrderDetails) // Lấy kèm chi tiết món hàng
                    .ThenInclude(od => od.Product) // Lấy kèm thông tin sản phẩm trong chi tiết
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Variant) // Lấy kèm size/màu
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.Id)
                .ToListAsync();

            return Ok(orders);
        }

        // 3. Lấy chi tiết 1 đơn hàng
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Variant)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound("Không tìm thấy đơn hàng");
            return Ok(order);
        }

        // 4. Admin lấy tất cả đơn hàng
        [HttpGet("admin/get-all")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _context.Orders
                .OrderByDescending(o => o.Id) // Sắp xếp mới nhất lên đầu
                .ToListAsync();
            return Ok(orders);
        }

        // 5. Cập nhật trạng thái đơn hàng
        [HttpPut("admin/update-status/{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromQuery] string status)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order == null) return NotFound("Đơn hàng không tồn tại");

                order.Status = status;
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