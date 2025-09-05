// Program.cs

using HRApi.Data;
using HRApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration; // Lấy configuration để sử dụng

// Thêm dịch vụ Controllers
builder.Services.AddControllers();

// ======================================================================
// === BẮT ĐẦU THÊM CẤU HÌNH XÁC THỰC JWT ===
// ======================================================================
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = configuration["Jwt:Audience"],
        ValidIssuer = configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
    };
});
// ======================================================================
// === KẾT THÚC CẤU HÌNH XÁC THỰC JWT ===
// ======================================================================


// Kết nối SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cấu hình CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy => policy.WithOrigins("http://localhost:3000")
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Sử dụng CORS
app.UseCors("AllowReactApp");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ======================================================================
// === THÊM 2 DÒNG NÀY (QUAN TRỌNG: PHẢI ĐÚNG THỨ TỰ) ===
// ======================================================================
app.UseAuthentication(); // <-- Middleware để xác thực người dùng dựa trên token
app.UseAuthorization();  // <-- Middleware để kiểm tra quyền hạn của người dùng đã xác thực
// ======================================================================

app.UseStaticFiles(); //cho phép hiển thị file ảnh nhân viên 

app.MapControllers();

// Phần Seeding database giữ nguyên
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();

        if (!context.Users.Any())
        {
            Console.WriteLine("Cơ sở dữ liệu trống, đang tạo tài khoản Admin mặc định...");

            var adminUser = new User
            {
                Username = "admin",
                Email = "admin@gmail.com",
                Password = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Role = "Admin",
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(adminUser);
            context.SaveChanges();

            Console.WriteLine("Tài khoản Admin đã được tạo thành công.");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Đã có lỗi xảy ra trong quá trình seeding database.");
    }
}

app.Run();