using HRApi.Data;
using HRApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace HRApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DangKyOTController : ControllerBase
    {
        private readonly AppDbContext _context;
        public DangKyOTController(AppDbContext context) { _context = context; }

        public class CreateOTDto
        {
            public DateTime NgayLamThem { get; set; }
            public TimeSpan GioBatDau { get; set; }
            public TimeSpan GioKetThuc { get; set; }
            public string LyDo { get; set; }
        }

        private string ConvertToUnSign(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            input = input.Trim().ToLower();
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = input.Normalize(NormalizationForm.FormD);
            return regex.Replace(temp, string.Empty).Replace('\u0111', 'd').Replace('\u0110', 'd');
        }

        [HttpPost]
        public async Task<IActionResult> CreateOT([FromBody] CreateOTDto dto)
        {
            var maNV = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (maNV == null) return Unauthorized();

            if (dto.GioKetThuc <= dto.GioBatDau)
                return BadRequest(new { message = "Giờ kết thúc phải lớn hơn giờ bắt đầu." });

            var soGio = (dto.GioKetThuc - dto.GioBatDau).TotalHours;

            var otRequest = new DangKyOT
            {
                MaNhanVien = maNV,
                NgayLamThem = dto.NgayLamThem,
                GioBatDau = dto.GioBatDau,
                GioKetThuc = dto.GioKetThuc,
                SoGio = soGio,
                LyDo = dto.LyDo,
                TrangThai = "Chờ duyệt",
                NgayGuiDon = DateTime.Now
            };

            _context.DangKyOTs.Add(otRequest);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đăng ký OT thành công" });
        }

        [HttpGet]
        [Authorize(Roles = "Trưởng phòng,Kế toán trưởng,Giám đốc,Tổng giám đốc")]
        public async Task<ActionResult<IEnumerable<object>>> GetAllRequests([FromQuery] string? trangThai)
        {
            var currentUserRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role")?.Value;
            var currentUserMaPhongBan = User.Claims.FirstOrDefault(c => c.Type == "MaPhongBan")?.Value;
            var roleClean = ConvertToUnSign(currentUserRole);

            var query = _context.DangKyOTs.Include(d => d.NhanVien).AsQueryable();

            if (roleClean == "truong phong")
            {
                if (!string.IsNullOrEmpty(currentUserMaPhongBan))
                    query = query.Where(d => d.NhanVien.MaPhongBan == currentUserMaPhongBan);
                else
                    return Ok(new List<object>());
            }

            if (!string.IsNullOrEmpty(trangThai))
            {
                query = query.Where(d => d.TrangThai == trangThai);
            }

            var result = await query.OrderByDescending(d => d.NgayGuiDon)
                .Select(d => new
                {
                    d.Id,
                    d.MaNhanVien,
                    HoTenNhanVien = d.NhanVien != null ? d.NhanVien.HoTen : "N/A",
                    d.NgayLamThem,
                    d.GioBatDau,
                    d.GioKetThuc,
                    d.SoGio,
                    d.LyDo,
                    d.TrangThai,
                    d.NgayGuiDon
                }).ToListAsync();

            return Ok(result);
        }

        [HttpPost("approve/{id}")]
        [Authorize(Roles = "Trưởng phòng,Kế toán trưởng,Giám đốc,Tổng giám đốc")]
        public async Task<IActionResult> Approve(int id)
        {
            var req = await _context.DangKyOTs.FindAsync(id);
            if (req == null || req.TrangThai != "Chờ duyệt") return NotFound("Đơn không hợp lệ.");

            req.TrangThai = "Đã duyệt";

            // Cập nhật ghi chú OT vào bảng chấm công để hiển thị
            var existingChamCong = await _context.ChamCongs
                .FirstOrDefaultAsync(c => c.MaNhanVien == req.MaNhanVien && c.NgayChamCong.Date == req.NgayLamThem.Date);

            string noteContent = $"OT (Đã duyệt): {req.SoGio}h";

            if (existingChamCong != null)
            {
                existingChamCong.GhiChu = string.IsNullOrEmpty(existingChamCong.GhiChu)
                    ? noteContent
                    : existingChamCong.GhiChu + "; " + noteContent;
            }
            else
            {
                // Nếu chưa có chấm công, tạo mới với công = 0 (vì lương OT tính riêng 300%)
                _context.ChamCongs.Add(new ChamCong
                {
                    MaNhanVien = req.MaNhanVien,
                    NgayChamCong = req.NgayLamThem.Date,
                    NgayCong = 0,
                    GhiChu = noteContent
                });
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã duyệt OT." });
        }

        [HttpPost("reject/{id}")]
        [Authorize(Roles = "Trưởng phòng,Kế toán trưởng,Giám đốc,Tổng giám đốc")]
        public async Task<IActionResult> Reject(int id)
        {
            var req = await _context.DangKyOTs.FindAsync(id);
            if (req == null) return NotFound();
            req.TrangThai = "Từ chối";
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã từ chối OT." });
        }
    }
}