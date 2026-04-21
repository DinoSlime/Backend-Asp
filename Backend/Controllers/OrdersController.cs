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
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
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
                await _context.SaveChangesAsync();

                var details = new List<OrderDetail>();

                foreach (var itemDTO in orderDTO.OrderDetails)
                {
                    long pId = (long)itemDTO.ProductId;
                    long vId = (long)itemDTO.VariantId;

                    var product = await _context.Products.FindAsync(pId);
                    if (product == null) throw new Exception($"Không tìm thấy sản phẩm ID: {pId}");

                    var variant = await _context.ProductVariants.FindAsync(vId);
                    if (variant == null) throw new Exception($"Không tìm thấy biến thể ID: {vId}");

                    if (variant.Stock < itemDTO.Quantity)
                    {
                        throw new Exception($"Sản phẩm {product.Name} (Size: {variant.Size}) không đủ hàng! Còn lại: {variant.Stock}");
                    }

                    variant.Stock -= itemDTO.Quantity;
                    _context.ProductVariants.Update(variant);

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

                _context.OrderDetails.AddRange(details);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(order);
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { message = e.Message });
            }
        }

        // 2. Lấy đơn hàng theo User ID
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetOrdersByUser(long userId)
        {
            var orders = await _context.Orders
                .Include(o => o.OrderDetails).ThenInclude(od => od.Product)
                .Include(o => o.OrderDetails).ThenInclude(od => od.Variant)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.Id)
                .ToListAsync();

            // 👇 LỌC DỮ LIỆU ĐỂ TRÁNH LỖI VÒNG LẶP JSON
            var cleanOrders = orders.Select(order => new
            {
                id = order.Id,
                fullName = order.FullName,
                customerName = order.FullName,
                phoneNumber = order.PhoneNumber,
                address = order.Address,
                totalMoney = order.TotalMoney,
                orderDate = order.OrderDate,
                status = order.Status,
                paymentMethod = order.PaymentMethod,
                orderDetails = order.OrderDetails.Select(od => new
                {
                    id = od.Id,
                    price = od.Price,
                    quantity = od.NumberOfProducts,
                    totalMoney = od.TotalMoney,
                    product = od.Product == null ? null : new { id = od.Product.Id, name = od.Product.Name, thumbnail = od.Product.Thumbnail },
                    variant = od.Variant == null ? null : new { id = od.Variant.Id, size = od.Variant.Size, color = od.Variant.Color }
                }).ToList()
            });

            return Ok(cleanOrders);
        }

        // 3. Lấy chi tiết 1 đơn hàng
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(long id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails).ThenInclude(od => od.Product)
                .Include(o => o.OrderDetails).ThenInclude(od => od.Variant)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound(new { message = "Không tìm thấy đơn hàng" });

            // 👇 LỌC DỮ LIỆU ĐỂ TRÁNH LỖI VÒNG LẶP JSON VÀ ÉP CHUẨN TÊN BIẾN
            var cleanData = new
            {
                id = order.Id,
                fullName = order.FullName,
                customerName = order.FullName, // Truyền 2 tên để đảm bảo React luôn đọc được
                phoneNumber = order.PhoneNumber,
                address = order.Address,
                note = order.Note,
                orderDate = order.OrderDate,
                status = order.Status,
                totalMoney = order.TotalMoney, // Biến này sẽ giúp mã QR hiện đúng tiền
                paymentMethod = order.PaymentMethod,
                orderDetails = order.OrderDetails.Select(od => new
                {
                    id = od.Id,
                    price = od.Price,
                    quantity = od.NumberOfProducts,
                    totalMoney = od.TotalMoney,
                    product = od.Product == null ? null : new { id = od.Product.Id, name = od.Product.Name, thumbnail = od.Product.Thumbnail },
                    variant = od.Variant == null ? null : new { id = od.Variant.Id, size = od.Variant.Size, color = od.Variant.Color }
                }).ToList()
            };

            return Ok(cleanData);
        }

        // 4. Admin lấy tất cả đơn hàng
        [HttpGet("admin/get-all")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _context.Orders
                .OrderByDescending(o => o.Id)
                .ToListAsync();

            // 👇 LỌC DỮ LIỆU CHO ADMIN (Chặn đứng lỗi trắng trang)
            var cleanOrders = orders.Select(o => new {
                id = o.Id,
                customerName = o.FullName,
                fullName = o.FullName,
                phoneNumber = o.PhoneNumber,
                address = o.Address,
                totalMoney = o.TotalMoney,
                orderDate = o.OrderDate,
                status = o.Status,
                paymentMethod = o.PaymentMethod
            });

            return Ok(cleanOrders);
        }

        // 5. Cập nhật trạng thái đơn hàng
        [HttpPut("admin/update-status/{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateOrderStatus(long id, [FromQuery] string status)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order == null) return NotFound(new { message = "Đơn hàng không tồn tại" });

                order.Status = status;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật trạng thái thành công" });
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        // Thêm hàm này vào OrdersController.cs
        [HttpPut("confirm-payment/{id}")]
        [Authorize] // Chỉ cần đăng nhập là được, không cần role ADMIN
        public async Task<IActionResult> ConfirmPayment(long id)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order == null) return NotFound(new { message = "Đơn hàng không tồn tại" });

                // Có thể thêm kiểm tra: chỉ cho phép xác nhận nếu đang ở trạng thái PENDING
                if (order.Status != "PENDING")
                {
                    return BadRequest(new { message = "Đơn hàng này không ở trạng thái chờ thanh toán" });
                }

                order.Status = "WAITING_CONFIRM";
                await _context.SaveChangesAsync();

                return Ok(new { message = "Đã gửi yêu cầu xác nhận thanh toán" });
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }
    }
}