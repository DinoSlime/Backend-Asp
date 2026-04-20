using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UploadsController : ControllerBase
    {
        private readonly Cloudinary _cloudinary;

        public UploadsController(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        [HttpPost("image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            try
            {
                // 1. Kiểm tra file rỗng
                if (file == null || file.Length == 0)
                {
                    return BadRequest("Vui lòng chọn file!");
                }

                // 2. Thiết lập thông số upload
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(file.FileName, file.OpenReadStream()),
                    Folder = "sneaker_products",
                    PublicId = Guid.NewGuid().ToString() // Tương đương UUID.randomUUID()
                };

                // 3. Thực hiện upload lên Cloudinary
                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    return StatusCode(500, uploadResult.Error.Message);
                }

                // 4. Trả về kết quả cho Frontend (Cấu trúc giống hệt Java của bạn)
                return Ok(new
                {
                    url = uploadResult.SecureUrl.ToString(),
                    publicId = uploadResult.PublicId
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, $"Lỗi upload file lên Cloud: {e.Message}");
            }
        }
    }
}