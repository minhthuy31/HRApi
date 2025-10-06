using HRApi.Data;
using HRApi.DTOs;
using HRApi.Models;
using Login.Models.DTOs;
using Login.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
namespace HRApi.Controllers;
using BCrypt.Net;


[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IEmailService _email;
    private readonly ITokenService _tokenSvc;

    public AuthController(AppDbContext db, IEmailService email, ITokenService tokenSvc)
    {
        _db = db;
        _email = email;
        _tokenSvc = tokenSvc;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        var exists = _db.NhanViens.Any(nv => nv.Email == req.Email);
        if (exists)
            return BadRequest(new { message = "Email đã tồn tại" });

        // Logic tạo mã nhân viên mới, bạn có thể tùy chỉnh
        var lastNv = _db.NhanViens.OrderByDescending(n => n.MaNhanVien).FirstOrDefault();
        int newId = (lastNv == null) ? 1 : int.Parse(lastNv.MaNhanVien.Substring(2)) + 1;
        string newMaNV = $"NV{newId:D4}";

        var nhanVien = new NhanVien
        {
            MaNhanVien = newMaNV,
            Email = req.Email,
            MatKhau = BCrypt.HashPassword(req.Password),
            TrangThai = true
        };

        _db.NhanViens.Add(nhanVien);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Đăng ký thành công" });
    }

    /// <summary>
    /// Đăng nhập bằng Email và Mật khẩu của Nhân Viên
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    public ActionResult<AuthResponse> Login([FromBody] LoginRequest req)
    {
        var nhanVien = _db.NhanViens.Include(u => u.UserRole).FirstOrDefault(u => u.Email == req.Email);

        Console.WriteLine($"Email nhập: {req.Email}");
        Console.WriteLine($"Có nhân viên? {nhanVien != null}");

        if (nhanVien != null)
        {
            Console.WriteLine($"Mật khẩu DB: {nhanVien.MatKhau}");
            Console.WriteLine($"Verify: {BCrypt.Verify(req.Password, nhanVien.MatKhau)}");
            Console.WriteLine($"TrangThai: {nhanVien.TrangThai}");
            Console.WriteLine($"Role: {nhanVien.UserRole?.NameRole ?? "Không có role"}");
        }

        if (nhanVien == null || string.IsNullOrEmpty(nhanVien.MatKhau) || !BCrypt.Verify(req.Password, nhanVien.MatKhau))
            return Unauthorized(new { message = "Sai email hoặc mật khẩu" });

        if (!nhanVien.TrangThai)
            return Unauthorized(new { message = "Tài khoản này đã bị vô hiệu hóa." });
        var roleName = nhanVien.UserRole?.NameRole;
        var maPhongBan = nhanVien.MaPhongBan;
        var jwt = _tokenSvc.CreateToken(nhanVien.MaNhanVien, nhanVien.Email, roleName, maPhongBan);
        return Ok(new AuthResponse
        {
            Token = jwt,
            Email = nhanVien.Email,
            MaNhanVien = nhanVien.MaNhanVien,
            HoTen = nhanVien.HoTen,
            Role = roleName,
            MaPhongBan = maPhongBan
        });
    }

    /// <summary>
    /// Gửi mã xác nhận quên mật khẩu(gửi về email)
    /// </summary>
    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req)
    {
        var nhanVien = _db.NhanViens.FirstOrDefault(u => u.Email == req.Email);
        if (nhanVien == null) return Ok(new { message = "Nếu email tồn tại, một mã xác nhận sẽ được gửi." });

        var code = Random.Shared.Next(100000, 999999).ToString();
        nhanVien.ResetCode = code;
        nhanVien.ResetCodeExpiry = DateTime.UtcNow.AddMinutes(10); // Thời gian sống của mã là 10 phút
        await _db.SaveChangesAsync();

        await _email.SendResetPasswordEmail(nhanVien.Email, code);
        return Ok(new { message = "Đã gửi mã xác nhận" });
    }

    /// <summary>
    /// Đặt lại mật khẩu bằng mã xác nhận
    /// </summary>
    /// 
    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
    {
        var nhanVien = _db.NhanViens.FirstOrDefault(u => u.Email == req.Email);
        if (nhanVien == null) return BadRequest(new { message = "Yêu cầu không hợp lệ." });

        if (nhanVien.ResetCode != req.Code || nhanVien.ResetCodeExpiry == null || nhanVien.ResetCodeExpiry < DateTime.UtcNow)
            return BadRequest(new { message = "Mã không hợp lệ hoặc đã hết hạn" });

        nhanVien.MatKhau = BCrypt.HashPassword(req.NewPassword);
        nhanVien.ResetCode = null;
        nhanVien.ResetCodeExpiry = null;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Đổi mật khẩu thành công" });
    }

    /// <summary>
    /// Lấy thông tin user hiện tại từ JWT
    /// </summary>
    /// 
    [AllowAnonymous]
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var emailClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var idClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        var maPhongBanClaim = User.Claims.FirstOrDefault(c => c.Type == "MaPhongBan")?.Value;
        var roleClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
        var nhanVien = _db.NhanViens.FirstOrDefault(nv => nv.Email == emailClaim);
        var authHeader = Request.Headers["Authorization"].ToString();
        var token = authHeader.StartsWith("Bearer ") ? authHeader.Substring("Bearer ".Length) : string.Empty;

        return Ok(new
        {
            Token = token,
            Email = emailClaim ?? "",
            MaNhanVien = idClaim ?? "",
            HoTen = nhanVien?.HoTen ?? "",
            MaPhongBan = maPhongBanClaim ?? "",
            Role = roleClaim ?? ""
        });
    }

    [AllowAnonymous]
    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest req)
    {
        // Lấy email từ token của người dùng đang đăng nhập
        var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        if (userEmail == null)
        {
            return Unauthorized();
        }

        var nhanVien = await _db.NhanViens.FirstOrDefaultAsync(nv => nv.Email == userEmail);
        if (nhanVien == null)
        {
            return NotFound("Không tìm thấy người dùng.");
        }
        if (string.IsNullOrEmpty(nhanVien.MatKhau) || !BCrypt.Verify(req.OldPassword, nhanVien.MatKhau))
        {
            return BadRequest(new { message = "Mật khẩu hiện tại không đúng." });
        }

        // Cập nhật mật khẩu mới
        nhanVien.MatKhau = BCrypt.HashPassword(req.NewPassword);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Đổi mật khẩu thành công." });
    }
}