using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;

namespace Backend.Controllers
{
    [Route("api/[controller]")] // Tương đương @RequestMapping("api/categories")
    [ApiController] // Tương đương @RestController
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        // Dependency Injection tương đương @RequiredArgsConstructor
        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Lấy tất cả danh mục (GET: api/categories)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetAllCategories()
        {
            // Tương đương categoryService.getAllCategories()
            return await _context.Categories.ToListAsync();
        }

        // 2. Tạo mới danh mục (POST: api/categories)
        [HttpPost]
        public async Task<ActionResult<Category>> CreateCategory([FromBody] Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return Ok(category);
        }

        // 3. Cập nhật danh mục (PUT: api/categories/{id})
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] Category category)
        {
            var existingCategory = await _context.Categories.FindAsync(id);
            if (existingCategory == null)
            {
                return NotFound("Không tìm thấy danh mục");
            }

            // Cập nhật thông tin (Giả sử bảng Category có trường Name)
            existingCategory.Name = category.Name;

            await _context.SaveChangesAsync();
            return Ok(existingCategory);
        }

        // 4. Xóa danh mục (DELETE: api/categories/{id})
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound("Không tìm thấy danh mục");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok($"Xóa thành công danh mục có ID: {id}");
        }
    }
}