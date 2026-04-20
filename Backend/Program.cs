using Backend.Data; // Nhúng thư mục Data
using Microsoft.EntityFrameworkCore; // Để dùng được UseSqlServer
using System.Text.Json.Serialization; // Để sửa lỗi vòng lặp JSON

var builder = WebApplication.CreateBuilder(args);

// 👇 1. ĐĂNG KÝ DATABASE (DbContext)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 👇 2. CẤU HÌNH CONTROLLER VÀ SỬA LỖI VÒNG LẶP JSON
builder.Services.AddControllers().AddJsonOptions(options =>
{
    // Cực kỳ quan trọng: Lệnh này thay thế hoàn toàn @JsonManagedReference và @JsonBackReference của Java
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

// Cấu hình Swagger (Tài liệu API)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();