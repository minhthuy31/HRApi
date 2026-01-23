using HRApi.Data;
using HRApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
            public decimal KinhPhiDuKien { get; set; }
            public decimal SoTienTamUng { get; set; }
            public string? LyDoTamUng { get; set; }
        }

        // --- TẠO ĐƠN (Giữ nguyên) ---
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

        // --- LẤY DANH SÁCH (Cập nhật Filter & Search & Phân quyền) ---
        [HttpGet]
        [Authorize(Roles = "Trưởng phòng,Kế toán trưởng,Giám đốc,Nhân sự trưởng")]
        public async Task<ActionResult<IEnumerable<object>>> GetAllRequests(
            [FromQuery] string? trangThai,
            [FromQuery] string? maPhongBan,
            [FromQuery] string? searchTerm)
        {
            var currentUserRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role")?.Value;
            var currentUserMaPhongBan = User.Claims.FirstOrDefault(c => c.Type == "MaPhongBan")?.Value;

            var query = _context.DangKyCongTacs
                .Include(d => d.NhanVien)
                .ThenInclude(nv => nv.PhongBan)
                .AsQueryable();

            // 1. PHÂN QUYỀN DATA
            if (currentUserRole == "Trưởng phòng")
            {
                if (!string.IsNullOrEmpty(currentUserMaPhongBan))
                {
                    query = query.Where(d => d.NhanVien.MaPhongBan == currentUserMaPhongBan);
                }
                else
                {
                    return Ok(new List<object>());
                }
            }
            // Admin, HR, Kế toán: Xem hết

            // 2. BỘ LỌC
            if (!string.IsNullOrEmpty(trangThai))
            {
                query = query.Where(d => d.TrangThai == trangThai);
            }

            if (!string.IsNullOrEmpty(maPhongBan))
            {
                query = query.Where(d => d.NhanVien.MaPhongBan == maPhongBan);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var lowerSearch = searchTerm.ToLower();
                query = query.Where(d => d.NhanVien.HoTen.ToLower().Contains(lowerSearch) ||
                                         d.MaNhanVien.ToLower().Contains(lowerSearch));
            }

            var result = await query.OrderByDescending(d => d.NgayGuiDon)
                .Select(d => new
                {
                    d.Id,
                    d.MaNhanVien,
                    HoTenNhanVien = d.NhanVien != null ? d.NhanVien.HoTen : "N/A",
                    TenPhongBan = d.NhanVien != null && d.NhanVien.PhongBan != null ? d.NhanVien.PhongBan.TenPhongBan : "N/A",
                    d.NgayBatDau,
                    d.NgayKetThuc,
                    d.NoiCongTac,
                    d.MucDich,
                    d.PhuongTien,
                    d.TrangThai,
                    d.NgayGuiDon,
                    d.KinhPhiDuKien,
                    d.SoTienTamUng,
                    d.LyDoTamUng
                }).ToListAsync();

            return Ok(result);
        }

        // --- DUYỆT ĐƠN (Chỉ Trưởng phòng & Giám đốc) ---
        [HttpPost("approve/{id}")]
        [Authorize(Roles = "Trưởng phòng,Giám đốc")] // Bỏ Kế toán, HR
        public async Task<IActionResult> Approve(int id)
        {
            var req = await _context.DangKyCongTacs.Include(d => d.NhanVien).FirstOrDefaultAsync(d => d.Id == id);
            if (req == null || req.TrangThai != "Chờ duyệt") return NotFound("Đơn không hợp lệ.");

            var currentUserRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role")?.Value;
            var currentUserMaPhongBan = User.Claims.FirstOrDefault(c => c.Type == "MaPhongBan")?.Value;

            // Check quyền Trưởng phòng
            if (currentUserRole == "Trưởng phòng")
            {
                if (req.NhanVien?.MaPhongBan != currentUserMaPhongBan)
                    return Forbid("Không được duyệt đơn phòng khác.");
            }

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

        // --- TỪ CHỐI ĐƠN (Chỉ Trưởng phòng & Giám đốc) ---
        [HttpPost("reject/{id}")]
        [Authorize(Roles = "Trưởng phòng,Giám đốc")] // Bỏ Kế toán, HR
        public async Task<IActionResult> Reject(int id)
        {
            var req = await _context.DangKyCongTacs.Include(d => d.NhanVien).FirstOrDefaultAsync(d => d.Id == id);
            if (req == null || req.TrangThai != "Chờ duyệt") return NotFound();

            var currentUserRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role")?.Value;
            var currentUserMaPhongBan = User.Claims.FirstOrDefault(c => c.Type == "MaPhongBan")?.Value;

            if (currentUserRole == "Trưởng phòng" && req.NhanVien?.MaPhongBan != currentUserMaPhongBan)
                return Forbid();

            req.TrangThai = "Từ chối";
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã từ chối đơn." });
        }
    }
}