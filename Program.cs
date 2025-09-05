// Thêm các using statement cần thiết
using HRApi.Data;
using HRApi.Models;
using HRApi.Services; // QUAN TRỌNG: Namespace cho các services
using Login.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models; // Cần cho Swagger
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Thêm dịch vụ Controllers
builder.Services.AddControllers();

// ======================================================================
// === BẮT ĐẦU CẤU HÌNH XÁC THỰC JWT (Giữ nguyên của bạn) ===
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
// === KẾT NỐI DATABASE VÀ CÁC DỊCH VỤ ===
// ======================================================================

// Kết nối SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// *** BẮT ĐẦU THÊM MỚI: Đăng ký EmailService và TokenService ***
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ITokenService, TokenService>();
// *** KẾT THÚC THÊM MỚI ***

// Cấu hình CORS (Giữ nguyên của bạn)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy => policy.WithOrigins("http://localhost:3000")
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

// ======================================================================
// === CẤU HÌNH SWAGGER (Nâng cấp) ===
// ======================================================================
builder.Services.AddEndpointsApiExplorer();
// Nâng cấp Swagger để hỗ trợ Authorize button
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "HRApi", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Nhập token theo format: Bearer {token}",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});


// ======================================================================
// === XÂY DỰNG ỨNG DỤNG VÀ CẤU HÌNH MIDDLEWARE ===
// ======================================================================
var app = builder.Build();

// Sử dụng CORS (Giữ nguyên của bạn)
app.UseCors("AllowReactApp");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Thứ tự các middleware này rất quan trọng (Giữ nguyên của bạn)
app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles(); //cho phép hiển thị file ảnh nhân viên

app.MapControllers();

// ======================================================================
// === SEEDING DATABASE (Giữ nguyên của bạn) ===
// GHI CHÚ QUAN TRỌNG:
// Đoạn code này đang tạo một tài khoản trong bảng "Users".
// Tuy nhiên, hệ thống đăng nhập mới của chúng ta đang dùng bảng "NhanViens".
// Bạn nên xem xét xóa bỏ Model "User" và đoạn code seeding này
// để tránh nhầm lẫn. Hoặc chỉnh sửa nó để tạo một Nhân Viên admin mặc định.
// ======================================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();

        // Kiểm tra xem bảng Users có tồn tại và có dữ liệu không
        if (context.Users != null && !context.Users.Any())
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