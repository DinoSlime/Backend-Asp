using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Tạo sản phẩm mới kèm Variants (POST: api/products)
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> CreateProduct([FromBody] Product product)
        {
            try
            {
                // Trong EF Core, khi bạn Add 1 Object cha (Product) có chứa danh sách con (Variants),
                // nó sẽ tự động thêm cả con vào Database mà không cần for-loop thủ công như Java.
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return Ok(product);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        // 2. Lấy tất cả có phân trang (GET: api/products?page=0&limit=10)
        [HttpGet]
        public async Task<IActionResult> GetAllProducts([FromQuery] int page = 0, [FromQuery] int limit = 100)
        {
            // Lấy tổng số để làm phân trang
            var total = await _context.Products.CountAsync();

                var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Variants)
            .OrderByDescending(p => p.Id) // 👈 Sản phẩm ID lớn nhất (mới nhất) sẽ lên đầu
            .ToListAsync();

            return Ok(new
            {
                data = products,
                total = total,
                page = page,
                limit = limit
            });
        }

        // 3. Lấy chi tiết sản phẩm theo ID (GET: api/products/5)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(long id)
        {
            var product = await _context.Products
                .Include(p => p.Variants) // Phải Include mới lấy được chi tiết Variants
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound("Không tìm thấy sản phẩm");

            return Ok(product);
        }

        // 4. Tìm kiếm nâng cao (GET: api/products/search?keyword=...)
        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts(
            [FromQuery] string keyword = "",
            [FromQuery] int? categoryId = null,
            [FromQuery] decimal minPrice = 0,
            [FromQuery] decimal maxPrice = 100000000,
            [FromQuery] int page = 0,
            [FromQuery] int limit = 10)
        {
            var query = _context.Products.Include(p => p.Variants).AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(p => p.Name.Contains(keyword) || p.Description.Contains(keyword));

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId);

            query = query.Where(p => p.Price >= minPrice && p.Price <= maxPrice);

            var totalItems = await query.CountAsync();
            var products = await query
                .OrderByDescending(p => p.Id)
                .Skip(page * limit)
                .Take(limit)
                .ToListAsync();

            return Ok(new { data = products, total = totalItems, page, limit });
        }

        // 5. Xóa sản phẩm (DELETE: api/products/5)
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteProduct(long id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            // Nếu DB của bạn thiết lập Cascade Delete, xóa Product sẽ tự xóa Variants
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Xóa thành công sản phẩm id: {id}" });
        }

        // 6. Cập nhật sản phẩm & Variants (PUT: api/products/5)
        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateProduct(long id, [FromBody] Product productData)
        {
            try
            {
                // Bước 1: Tìm sản phẩm cũ, bắt buộc phải Include Variants
                var existingProduct = await _context.Products
                    .Include(p => p.Variants)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (existingProduct == null) return NotFound("Không tìm thấy sản phẩm");

                // Bước 2: Cập nhật thông tin cơ bản
                existingProduct.Name = productData.Name;
                existingProduct.Price = productData.Price;
                existingProduct.Description = productData.Description;
                existingProduct.CategoryId = productData.CategoryId;
                existingProduct.Thumbnail = productData.Thumbnail;

                // Bước 3: Cập nhật Variants (Xóa cũ, Thêm mới - giống logic Java)
                if (productData.Variants != null)
                {
                    // Lệnh này sẽ xóa các variants hiện tại ra khỏi Database
                    _context.RemoveRange(existingProduct.Variants);

                    // Thêm variants mới từ request gửi lên
                    foreach (var variant in productData.Variants)
                    {
                        variant.Id = 0; // Đặt ID = 0 để EF Core hiểu đây là dữ liệu cần thêm mới (INSERT)
                        variant.ProductId = existingProduct.Id; // Liên kết với sản phẩm cha
                        existingProduct.Variants.Add(variant);
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(existingProduct);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}