using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;

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

        // 1. Tạo sản phẩm mới (POST: api/products)
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] Product product)
        {
            try
            {
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
        public async Task<IActionResult> GetAllProducts(
            [FromQuery] int page = 0,
            [FromQuery] int limit = 10)
        {
            var products = await _context.Products
                .OrderByDescending(p => p.Id) // Thay cho Sort.by("createdAt").descending()
                .Skip(page * limit)           // Bỏ qua các bản ghi trang trước
                .Take(limit)                  // Lấy số lượng bản ghi giới hạn
                .ToListAsync();

            var totalItems = await _context.Products.CountAsync();

            return Ok(new { data = products, total = totalItems, page, limit });
        }

        // 3. Lấy chi tiết sản phẩm theo ID (GET: api/products/5)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound("Không tìm thấy sản phẩm");

            // C# thường trả về dữ liệu thuần, ít dùng HATEOAS (linkTo) như Java 
            // nhưng bạn có thể thêm thủ công nếu Frontend yêu cầu
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
            // Xây dựng query động bằng LINQ (Tương đương SearchService bên Java)
            var query = _context.Products.AsQueryable();

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
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return Ok($"Xóa thành công sản phẩm id: {id}");
        }

        // 6. Cập nhật sản phẩm (PUT: api/products/5)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product productData)
        {
            try
            {
                var existingProduct = await _context.Products.FindAsync(id);
                if (existingProduct == null) return NotFound();

                // Cập nhật các trường dữ liệu
                existingProduct.Name = productData.Name;
                existingProduct.Price = productData.Price;
                existingProduct.Description = productData.Description;
                existingProduct.CategoryId = productData.CategoryId;
                existingProduct.Thumbnail = productData.Thumbnail;

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