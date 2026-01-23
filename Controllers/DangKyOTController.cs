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

        // POST: api/DangKyOT
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

        // GET: api/DangKyOT (Có Filter & Search & Phân quyền)
        [HttpGet]
        [Authorize(Roles = "Trưởng phòng,Kế toán trưởng,Giám đốc,Nhân sự trưởng")]
        public async Task<ActionResult<IEnumerable<object>>> GetAllRequests(
            [FromQuery] string? trangThai,
            [FromQuery] string? maPhongBan,
            [FromQuery] string? searchTerm)
        {
            var currentUserRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role")?.Value;
            var currentUserMaPhongBan = User.Claims.FirstOrDefault(c => c.Type == "MaPhongBan")?.Value;

            var query = _context.DangKyOTs
                .Include(d => d.NhanVien)
                .ThenInclude(nv => nv.PhongBan) // Include phòng ban để filter/hiển thị
                .AsQueryable();

            // --- 1. PHÂN QUYỀN DATA ---
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
            // Các role khác (Admin, HR, Kế toán) xem hết

            // --- 2. BỘ LỌC ---
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

        // --- DUYỆT OT (Chỉ Trưởng phòng & Giám đốc) ---
        [HttpPost("approve/{id}")]
        [Authorize(Roles = "Trưởng phòng,Giám đốc")]
        public async Task<IActionResult> Approve(int id)
        {
            var req = await _context.DangKyOTs.Include(d => d.NhanVien).FirstOrDefaultAsync(d => d.Id == id);
            if (req == null || req.TrangThai != "Chờ duyệt") return NotFound("Đơn không hợp lệ.");

            var currentUserRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role")?.Value;
            var currentUserMaPhongBan = User.Claims.FirstOrDefault(c => c.Type == "MaPhongBan")?.Value;

            // Check quyền Trưởng phòng: Không được duyệt khác phòng
            if (currentUserRole == "Trưởng phòng")
            {
                if (req.NhanVien?.MaPhongBan != currentUserMaPhongBan)
                    return Forbid("Không được duyệt đơn phòng khác.");
            }

            req.TrangThai = "Đã duyệt";

            // Cập nhật ghi chú OT vào bảng chấm công
            var existingChamCong = await _context.ChamCongs
                .FirstOrDefaultAsync(c => c.MaNhanVien == req.MaNhanVien && c.NgayChamCong.Date == req.NgayLamThem.Date);

            string noteContent = $"OT (Đã duyệt): {req.SoGio}h";

            if (existingChamCong != null)
            {
                // Nếu đã có chấm công, nối thêm ghi chú (để không mất dữ liệu chấm công chính)
                existingChamCong.GhiChu = string.IsNullOrEmpty(existingChamCong.GhiChu)
                    ? noteContent
                    : existingChamCong.GhiChu + "; " + noteContent;
            }
            else
            {
                // Nếu chưa có chấm công, tạo mới
                _context.ChamCongs.Add(new ChamCong
                {
                    MaNhanVien = req.MaNhanVien,
                    NgayChamCong = req.NgayLamThem.Date,
                    NgayCong = 0, // OT tính riêng, không cộng vào ngày công chuẩn
                    GhiChu = noteContent
                });
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã duyệt OT." });
        }

        // --- TỪ CHỐI OT (Chỉ Trưởng phòng & Giám đốc) ---
        [HttpPost("reject/{id}")]
        [Authorize(Roles = "Trưởng phòng,Giám đốc")]
        public async Task<IActionResult> Reject(int id)
        {
            var req = await _context.DangKyOTs.Include(d => d.NhanVien).FirstOrDefaultAsync(d => d.Id == id);
            if (req == null || req.TrangThai != "Chờ duyệt") return NotFound();

            var currentUserRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role")?.Value;
            var currentUserMaPhongBan = User.Claims.FirstOrDefault(c => c.Type == "MaPhongBan")?.Value;

            if (currentUserRole == "Trưởng phòng" && req.NhanVien?.MaPhongBan != currentUserMaPhongBan)
                return Forbid();

            req.TrangThai = "Từ chối";
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã từ chối OT." });
        }
    }
}