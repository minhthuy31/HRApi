using HRApi.Data;
using HRApi.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HRApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration; // Inject IConfiguration

        public AuthController(AppDbContext context, IConfiguration configuration) // Sửa constructor
        {
            _context = context;
            _configuration = configuration; // Sửa constructor
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == loginDto.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password))
            {
                return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu không đúng!" });
            }

            // --- TẠO TOKEN THẬT ---
            var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return Ok(new
            {
                message = "Đăng nhập thành công!",
                token = new JwtSecurityTokenHandler().WriteToken(token), // Trả về token thật
                expiration = token.ValidTo
            });
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == changePasswordDto.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(changePasswordDto.OldPassword, user.Password))
            {
                return BadRequest(new { message = "Tên đăng nhập hoặc mật khẩu cũ không đúng." });
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đổi mật khẩu thành công!" });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == forgotPasswordDto.Username);

            if (user != null)
            {
                user.PasswordResetToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
                user.ResetTokenExpires = DateTime.UtcNow.AddMinutes(15);
                await _context.SaveChangesAsync();

                Console.WriteLine($"An email would be sent to {user.Email} with token: {user.PasswordResetToken}");
            }

            return Ok(new { message = "Nếu tên đăng nhập tồn tại trong hệ thống, một liên kết đặt lại mật khẩu đã được gửi đến email đã đăng ký." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.PasswordResetToken == resetPasswordDto.Token &&
                u.ResetTokenExpires > DateTime.UtcNow);

            if (user == null)
            {
                return BadRequest(new { message = "Token không hợp lệ hoặc đã hết hạn." });
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(resetPasswordDto.NewPassword);

            user.PasswordResetToken = null;
            user.ResetTokenExpires = null;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Mật khẩu đã được đặt lại thành công." });
        }
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            // Lấy username từ token đã được xác thực
            var username = User.FindFirst(ClaimTypes.Name)?.Value;

            if (username == null)
            {
                return Unauthorized();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return NotFound();
            }
            return Ok(new { user.Id, user.Username, user.Email, user.Role, user.CreatedAt });
        }
    }
}