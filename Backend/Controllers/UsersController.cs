using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
using Backend.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public UsersController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // 1. Đăng ký (POST: api/users/register)
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDTO dto)
        {
            try
            {
                // 1. Kiểm tra mật khẩu khớp (Đã comment theo yêu cầu)
                /*
                if (dto.Password != dto.RetypePassword)
                    return BadRequest("Mật khẩu xác nhận không khớp!");
                */

                // 2. Kiểm tra username tồn tại
                if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
                    return BadRequest("Tên tài khoản đã tồn tại!");

                // 3. Kiểm tra xem Role USER (Id = 2) có tồn tại trong DB chưa
                // ✅ Đã sửa: Dùng 2L để khớp kiểu dữ liệu long
                var defaultRole = await _context.Roles.FindAsync(2L);
                if (defaultRole == null)
                {
                    return BadRequest("Hệ thống chưa cấu hình Role USER (ID=2). Vui lòng chèn Role vào DB trước!");
                }

                // 4. Map DTO sang Entity
                var user = new User
                {
                    FullName = dto.FullName,
                    Username = dto.Username,
                    Password = dto.Password,
                    PhoneNumber = dto.PhoneNumber,
                    Address = dto.Address,
                    RoleId = 2L, // ✅ Đã sửa: Dùng 2L cho kiểu dữ liệu long
                    IsActive = true
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Đăng ký thành công tài khoản USER!", username = user.Username });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        // 2. Đăng nhập (POST: api/users/login)
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDTO dto)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Username == dto.Username && u.Password == dto.Password);

                if (user == null)
                    return BadRequest("Sai tài khoản hoặc mật khẩu!");

                if (!user.IsActive)
                    return BadRequest("Tài khoản của bạn đã bị khóa!");

                string token = GenerateJwtToken(user);

                var result = new
                {
                    token = token,
                    user = new
                    {
                        id = user.Id,
                        username = user.Username,
                        fullName = user.FullName,
                        role = user.Role?.Name?.ToUpper() ?? "USER"
                    }
                };

                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        // 3. Lấy toàn bộ danh sách User (Chỉ dành cho ADMIN)
        [HttpGet("admin/all-users")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _context.Users
                    .Include(u => u.Role)
                    .Select(u => new {
                        u.Id,
                        u.FullName,
                        u.Username,
                        u.PhoneNumber,
                        u.Address,
                        u.IsActive,
                        RoleName = u.Role != null ? u.Role.Name : "N/A"
                    })
                    .ToListAsync();

                return Ok(users);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        // 4. Xóa người dùng (DELETE: api/users/{id})
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteUser(long id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("Người dùng không tồn tại");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Xóa người dùng thành công" });
        }

        // 5. Cập nhật thông tin (PUT: api/users/{id})
        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateUser(long id, [FromBody] User updatedData)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("Người dùng không tồn tại");

            user.FullName = updatedData.FullName;
            user.PhoneNumber = updatedData.PhoneNumber;
            user.Address = updatedData.Address;
            user.RoleId = updatedData.RoleId;
            user.IsActive = updatedData.IsActive;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật thành công" });
        }

        // 👇 HÀM TẠO TOKEN
        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role?.Name ?? "USER")
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}