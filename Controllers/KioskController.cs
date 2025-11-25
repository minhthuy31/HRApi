using HRApi.Data;
using HRApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRApi.Controllers
{
    [AllowAnonymous] // Cho phép Kiosk gọi mà không cần đăng nhập
    [ApiController]
    [Route("api/[controller]")]
    public class KioskController : ControllerBase
    {
        private readonly AppDbContext _context;
        public KioskController(AppDbContext context) { _context = context; }

        [HttpGet("generate-token")]
        public async Task<IActionResult> GenerateToken()
        {
            var tokenString = Guid.NewGuid().ToString("N"); // Tạo chuỗi ngẫu nhiên

            var newQrToken = new ActiveQRToken
            {
                Token = tokenString,
                ExpiresAt = DateTime.UtcNow.AddSeconds(310), // Cho phép 310 giay 
                IsUsed = false
            };

            _context.ActiveQRTokens.Add(newQrToken);
            await _context.SaveChangesAsync();

            // Trả về chuỗi này để Kiosk biến thành mã QR
            return Ok(new { token = tokenString });
        }
    }
}
