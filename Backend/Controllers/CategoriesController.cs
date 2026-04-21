using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Lấy tất cả danh mục
        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _context.Categories.ToListAsync();
            return Ok(categories);
        }

        // 2. Lấy chi tiết danh mục theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(long id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound(new { message = $"Không tìm thấy Category với ID: {id}" });
            }
            return Ok(category);
        }

        // 3. Tạo mới danh mục
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> CreateCategory([FromBody] Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return Ok(category);
        }

        // 4. Cập nhật danh mục
        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateCategory(long id, [FromBody] Category categoryDetails)
        {
            var existingCategory = await _context.Categories.FindAsync(id);
            if (existingCategory == null)
            {
                return NotFound(new { message = "Không tìm thấy danh mục" });
            }

            // 👇 LOGIC NGHIỆP VỤ: Nếu danh mục có sản phẩm, cân nhắc việc có cho đổi tên hay không
            // Thường thì Update tên vẫn cho phép, nhưng nếu bạn muốn siết chặt:
            var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);
            if (hasProducts)
            {
                // Bạn có thể chọn trả về lỗi hoặc cho phép sửa tên nhưng cảnh báo.
                // Ở đây mình vẫn cho sửa tên (vì không ảnh hưởng liên kết), nhưng nếu logic của bạn cấm thì dùng đoạn dưới:
                // return BadRequest(new { message = "Danh mục đang có sản phẩm, không thể chỉnh sửa thông tin!" });
            }

            existingCategory.Name = categoryDetails.Name;
            await _context.SaveChangesAsync();
            return Ok(existingCategory);
        }

        // 5. Xóa danh mục (QUAN TRỌNG NHẤT)
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteCategory(long id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound(new { message = $"Không tìm thấy Category để xóa với ID: {id}" });
            }

            // 👇 LOGIC NGHIỆP VỤ: Kiểm tra xem có sản phẩm nào thuộc Category này không
            var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);

            if (hasProducts)
            {
                // Trả về BadRequest (400) để báo lỗi nghiệp vụ
                return BadRequest(new
                {
                    message = "Không thể xóa danh mục này vì vẫn còn sản phẩm thuộc danh mục. Vui lòng xóa hoặc chuyển các sản phẩm sang danh mục khác trước!"
                });
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Xóa thành công danh mục có ID: {id}" });
        }
    }
}