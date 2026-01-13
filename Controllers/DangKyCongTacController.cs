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
    public class DangKyCongTacController : ControllerBase
    {
        private readonly AppDbContext _context;
        public DangKyCongTacController(AppDbContext context) { _context = context; }

        public class CreateCongTacDto
        {
            public DateTime NgayBatDau { get; set; }
            public DateTime NgayKetThuc { get; set; }
            public string NoiCongTac { get; set; }
            public string MucDich { get; set; }
            public string? PhuongTien { get; set; }

            // Thêm fields kinh phí
            public decimal KinhPhiDuKien { get; set; }
            public decimal SoTienTamUng { get; set; }
            public string? LyDoTamUng { get; set; }
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
        public async Task<IActionResult> Create([FromBody] CreateCongTacDto dto)
        {
            var maNV = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (maNV == null) return Unauthorized();

            if (dto.NgayKetThuc < dto.NgayBatDau)
                return BadRequest(new { message = "Ngày kết thúc không hợp lệ." });

            var req = new DangKyCongTac
            {
                MaNhanVien = maNV,
                NgayBatDau = dto.NgayBatDau,
                NgayKetThuc = dto.NgayKetThuc,
                NoiCongTac = dto.NoiCongTac,
                MucDich = dto.MucDich,
                PhuongTien = dto.PhuongTien,
                // Map kinh phí
                KinhPhiDuKien = dto.KinhPhiDuKien,
                SoTienTamUng = dto.SoTienTamUng,
                LyDoTamUng = dto.LyDoTamUng,

                TrangThai = "Chờ duyệt",
                NgayGuiDon = DateTime.Now
            };

            _context.DangKyCongTacs.Add(req);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đăng ký công tác thành công" });
        }

        [HttpGet]
        [Authorize(Roles = "Trưởng phòng,Kế toán trưởng,Giám đốc,Tổng giám đốc")]
        public async Task<ActionResult<IEnumerable<object>>> GetAllRequests([FromQuery] string? trangThai)
        {
            var currentUserRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role")?.Value;
            var currentUserMaPhongBan = User.Claims.FirstOrDefault(c => c.Type == "MaPhongBan")?.Value;
            var roleClean = ConvertToUnSign(currentUserRole);

            var query = _context.DangKyCongTacs.Include(d => d.NhanVien).AsQueryable();

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
                    d.NgayBatDau,
                    d.NgayKetThuc,
                    d.NoiCongTac,
                    d.MucDich,
                    d.PhuongTien,
                    d.TrangThai,
                    d.NgayGuiDon,
                    // Trả về thông tin kinh phí
                    d.KinhPhiDuKien,
                    d.SoTienTamUng,
                    d.LyDoTamUng
                }).ToListAsync();

            return Ok(result);
        }

        [HttpPost("approve/{id}")]
        [Authorize(Roles = "Trưởng phòng,Kế toán trưởng,Giám đốc,Tổng giám đốc")]
        public async Task<IActionResult> Approve(int id)
        {
            var req = await _context.DangKyCongTacs.FindAsync(id);
            if (req == null || req.TrangThai != "Chờ duyệt") return NotFound("Đơn không hợp lệ.");

            req.TrangThai = "Đã duyệt";

            // Tự động tính công 1.0 (không trừ lương) cho các ngày công tác
            for (var date = req.NgayBatDau; date.Date <= req.NgayKetThuc.Date; date = date.AddDays(1))
            {
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday) continue;

                var existingChamCong = await _context.ChamCongs
                    .FirstOrDefaultAsync(c => c.MaNhanVien == req.MaNhanVien && c.NgayChamCong.Date == date.Date);

                string noteContent = $"Công tác: {req.NoiCongTac}";

                if (existingChamCong != null)
                {
                    existingChamCong.NgayCong = 1.0;
                    existingChamCong.GhiChu = string.IsNullOrEmpty(existingChamCong.GhiChu)
                        ? noteContent
                        : existingChamCong.GhiChu + "; " + noteContent;
                }
                else
                {
                    _context.ChamCongs.Add(new ChamCong
                    {
                        MaNhanVien = req.MaNhanVien,
                        NgayChamCong = date.Date,
                        NgayCong = 1.0,
                        GhiChu = noteContent
                    });
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã duyệt đơn công tác." });
        }

        [HttpPost("reject/{id}")]
        [Authorize(Roles = "Trưởng phòng,Kế toán trưởng,Giám đốc,Tổng giám đốc")]
        public async Task<IActionResult> Reject(int id)
        {
            var req = await _context.DangKyCongTacs.FindAsync(id);
            if (req == null) return NotFound();
            req.TrangThai = "Từ chối";
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã từ chối đơn." });
        }
    }
}