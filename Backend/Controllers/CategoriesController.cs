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

        // 1. Lấy tất cả danh mục (GET: api/categories)
        // Public: Ai cũng xem được
        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _context.Categories.ToListAsync();
            return Ok(categories);
        }

        // 2. Lấy chi tiết danh mục theo ID (GET: api/categories/{id}) 
        // 👇 ĐÂY LÀ PHẦN BỔ SUNG TỪ BÊN JAVA SANG
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound(new { message = $"Không tìm thấy Category với ID: {id}" });
            }
            return Ok(category);
        }

        // 3. Tạo mới danh mục (POST: api/categories)
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> CreateCategory([FromBody] Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return Ok(category);
        }

        // 4. Cập nhật danh mục (PUT: api/categories/{id})
        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] Category categoryDetails)
        {
            var existingCategory = await _context.Categories.FindAsync(id);
            if (existingCategory == null)
            {
                return NotFound(new { message = "Không tìm thấy danh mục" });
            }

            // Cập nhật thông tin
            existingCategory.Name = categoryDetails.Name;

            await _context.SaveChangesAsync();
            return Ok(existingCategory);
        }

        // 5. Xóa danh mục (DELETE: api/categories/{id})
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound(new { message = $"Không tìm thấy Category để xóa với ID: {id}" });
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Xóa thành công danh mục có ID: {id}" });
        }
    }
}