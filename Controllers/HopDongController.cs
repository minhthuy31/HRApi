using HRApi.Data;
using HRApi.DTOs;
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
    public class HopDongController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public HopDongController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: api/HopDong
        // Ai cũng xem được (nhưng có logic lọc dữ liệu bên trong hàm)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetHopDongs(
            [FromQuery] string? search,
            [FromQuery] string? trangThai
        )
        {
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role")?.Value;
            var userDept = User.Claims.FirstOrDefault(c => c.Type == "MaPhongBan")?.Value;

            var query = _context.HopDongs
                .Include(h => h.NhanVien).ThenInclude(nv => nv.PhongBan)
                .Include(h => h.NhanVien).ThenInclude(nv => nv.ChucVuNhanVien)
                .AsQueryable();

            // 1. Phân quyền xem
            if (userRole == "Trưởng phòng")
            {
                if (!string.IsNullOrEmpty(userDept))
                    query = query.Where(h => h.NhanVien.MaPhongBan == userDept);
                else
                    return Ok(new List<object>());
            }
            // Giám đốc, HR, Kế toán: Xem hết

            // 2. Tìm kiếm
            if (!string.IsNullOrEmpty(search))
            {
                var lowerSearch = search.ToLower();
                query = query.Where(h =>
                    h.MaNhanVien.ToLower().Contains(lowerSearch) ||
                    (h.NhanVien != null && h.NhanVien.HoTen.ToLower().Contains(lowerSearch)) ||
                    h.SoHopDong.ToLower().Contains(lowerSearch));
            }

            // 3. Lọc trạng thái
            if (!string.IsNullOrEmpty(trangThai) && trangThai != "All")
            {
                query = query.Where(h => h.TrangThai == trangThai);
            }
            else if (string.IsNullOrEmpty(trangThai))
            {
                query = query.Where(h => h.TrangThai == "HieuLuc");
            }

            var result = await query.OrderByDescending(h => h.NgayBatDau)
                .Select(h => new
                {
                    h.SoHopDong,
                    h.MaNhanVien,
                    HoTenNhanVien = h.NhanVien != null ? h.NhanVien.HoTen : "N/A",
                    TenPhongBan = h.NhanVien != null && h.NhanVien.PhongBan != null ? h.NhanVien.PhongBan.TenPhongBan : "",
                    TenChucVu = h.NhanVien != null && h.NhanVien.ChucVuNhanVien != null ? h.NhanVien.ChucVuNhanVien.TenChucVu : "",
                    NgaySinh = h.NhanVien != null ? h.NhanVien.NgaySinh : null,
                    CCCD = h.NhanVien != null ? h.NhanVien.CCCD : "",
                    DiaChi = h.NhanVien != null ? h.NhanVien.DiaChiThuongTru : "",
                    SoDienThoai = h.NhanVien != null ? h.NhanVien.sdt_NhanVien : "",
                    h.LoaiHopDong,
                    h.NgayBatDau,
                    h.NgayKetThuc,
                    h.LuongCoBan,
                    h.TrangThai,
                    h.TepDinhKem,
                    h.GhiChu
                })
                .ToListAsync();

            return Ok(result);
        }

        // POST: api/HopDong
        // --- CHỈ GIÁM ĐỐC & NHÂN SỰ TRƯỞNG MỚI ĐƯỢC TẠO ---
        [HttpPost]
        [Authorize(Roles = "Giám đốc,Nhân sự trưởng")]
        public async Task<ActionResult<HopDong>> CreateHopDong([FromForm] HopDongInputDto dto)
        {
            if (await _context.HopDongs.AnyAsync(h => h.SoHopDong == dto.SoHopDong))
                return BadRequest(new { message = $"Số hợp đồng '{dto.SoHopDong}' đã tồn tại." });

            var nhanVien = await _context.NhanViens.FindAsync(dto.MaNhanVien);
            if (nhanVien == null) return BadRequest(new { message = "Mã nhân viên không tồn tại." });

            string? filePath = null;
            if (dto.FileDinhKem != null && dto.FileDinhKem.Length > 0)
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "contracts");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                string uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.FileDinhKem.FileName)}";
                using (var stream = new FileStream(Path.Combine(uploadsFolder, uniqueFileName), FileMode.Create))
                {
                    await dto.FileDinhKem.CopyToAsync(stream);
                }
                filePath = "/contracts/" + uniqueFileName;
            }

            var hopDong = new HopDong
            {
                SoHopDong = dto.SoHopDong,
                MaNhanVien = dto.MaNhanVien,
                LoaiHopDong = dto.LoaiHopDong,
                NgayBatDau = dto.NgayBatDau,
                NgayKetThuc = dto.NgayKetThuc,
                LuongCoBan = dto.LuongCoBan,
                TepDinhKem = filePath,
                TrangThai = dto.TrangThai,
                GhiChu = dto.GhiChu,
                NgayKy = DateTime.Now
            };

            _context.HopDongs.Add(hopDong);

            // Sync thông tin sang Nhân viên
            nhanVien.LuongCoBan = dto.LuongCoBan;
            nhanVien.SoHopDong = dto.SoHopDong;
            nhanVien.LoaiNhanVien = dto.LoaiHopDong;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Tạo hợp đồng thành công" });
        }

        // PUT: api/HopDong
        // --- CHỈ GIÁM ĐỐC & NHÂN SỰ TRƯỞNG MỚI ĐƯỢC SỬA ---
        [HttpPut]
        [Authorize(Roles = "Giám đốc,Nhân sự trưởng")]
        public async Task<IActionResult> UpdateHopDong([FromQuery] string id, [FromForm] HopDongInputDto dto)
        {
            var hopDong = await _context.HopDongs.FindAsync(id);
            if (hopDong == null) return NotFound(new { message = $"Không tìm thấy hợp đồng số '{id}'" });

            if (dto.FileDinhKem != null && dto.FileDinhKem.Length > 0)
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "contracts");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                string uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.FileDinhKem.FileName)}";
                using (var stream = new FileStream(Path.Combine(uploadsFolder, uniqueFileName), FileMode.Create))
                {
                    await dto.FileDinhKem.CopyToAsync(stream);
                }
                hopDong.TepDinhKem = "/contracts/" + uniqueFileName;
            }

            hopDong.LoaiHopDong = dto.LoaiHopDong;
            hopDong.NgayBatDau = dto.NgayBatDau;
            hopDong.NgayKetThuc = dto.NgayKetThuc;
            hopDong.LuongCoBan = dto.LuongCoBan;
            hopDong.TrangThai = dto.TrangThai;
            hopDong.GhiChu = dto.GhiChu;

            // Chỉ cập nhật lương cho nhân viên nếu HĐ này ĐANG HIỆU LỰC
            if (hopDong.TrangThai == "HieuLuc")
            {
                var nhanVien = await _context.NhanViens.FindAsync(hopDong.MaNhanVien);
                if (nhanVien != null)
                {
                    nhanVien.LuongCoBan = dto.LuongCoBan;
                    nhanVien.LoaiNhanVien = dto.LoaiHopDong;
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật thành công" });
        }

        // DELETE: api/HopDong (Tùy chọn: Có thể bỏ nếu không dùng, hoặc cũng khóa quyền)
        [HttpDelete]
        [Authorize(Roles = "Giám đốc,Nhân sự trưởng")]
        public async Task<IActionResult> DeleteHopDong([FromQuery] string id)
        {
            var hd = await _context.HopDongs.FindAsync(id);
            if (hd == null) return NotFound(new { message = "Không tìm thấy hợp đồng." });
            _context.HopDongs.Remove(hd);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã xóa hợp đồng" });
        }
    }
}