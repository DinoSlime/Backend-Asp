using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
using Backend.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration; // 👇 Thêm cấu hình

        public UsersController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration; // 👇 Inject Configuration
        }

        // 1. Đăng ký (POST: api/users/register)
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDTO dto)
        {
            try
            {
                if (dto.Password != dto.RetypePassword)
                    return BadRequest("Mật khẩu xác nhận không khớp!");

                if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
                    return BadRequest("Tên tài khoản đã tồn tại!");

                var user = new User
                {
                    FullName = dto.FullName,
                    Username = dto.Username,
                    Password = dto.Password,
                    PhoneNumber = dto.PhoneNumber,
                    Address = dto.Address,
                    RoleId = 2
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return Ok(user);
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

                // 👇 2. TẠO TOKEN THẬT
                string token = GenerateJwtToken(user);

                // 3. Trả về kết quả
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

        // 👇 HÀM TẠO TOKEN (Viết riêng để tái sử dụng)
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