using HRApi.Data;
using HRApi.DTOs;
using HRApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetHopDongs([FromQuery] string? search)
        {
            var query = _context.HopDongs
                .Include(h => h.NhanVien)
                .ThenInclude(nv => nv.PhongBan) // <--- QUAN TRỌNG: Lấy thêm thông tin Phòng Ban
                .Include(h => h.NhanVien)
                .ThenInclude(nv => nv.ChucVuNhanVien) // Lấy thêm Chức vụ
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                var lowerSearch = search.ToLower();
                query = query.Where(h =>
                    h.MaNhanVien.ToLower().Contains(lowerSearch) ||
                    (h.NhanVien != null && h.NhanVien.HoTen.ToLower().Contains(lowerSearch)) ||
                    h.SoHopDong.ToLower().Contains(lowerSearch));
            }

            var result = await query.OrderByDescending(h => h.NgayBatDau)
                .Select(h => new
                {
                    h.SoHopDong,
                    h.MaNhanVien,
                    HoTenNhanVien = h.NhanVien != null ? h.NhanVien.HoTen : "N/A",
                    TenPhongBan = h.NhanVien != null && h.NhanVien.PhongBan != null ? h.NhanVien.PhongBan.TenPhongBan : "",
                    TenChucVu = h.NhanVien != null && h.NhanVien.ChucVuNhanVien != null ? h.NhanVien.ChucVuNhanVien.TenChucVu : "",
                    SoDienThoai = h.NhanVien != null ? h.NhanVien.sdt_NhanVien : "",
                    CCCD = h.NhanVien != null ? h.NhanVien.CCCD : "",
                    DiaChi = h.NhanVien != null ? h.NhanVien.DiaChiThuongTru : "",
                    NgaySinh = h.NhanVien != null ? h.NhanVien.NgaySinh : null,

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

        // ... (Các phương thức POST, PUT, DELETE giữ nguyên như cũ) ...
        // POST: api/HopDong
        [HttpPost]
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

            // Sync thông tin về nhân viên
            nhanVien.LuongCoBan = dto.LuongCoBan;
            nhanVien.SoHopDong = dto.SoHopDong;
            nhanVien.LoaiNhanVien = dto.LoaiHopDong;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Tạo hợp đồng thành công" });
        }

        // PUT: api/HopDong
        [HttpPut]
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

        // DELETE: api/HopDong
        [HttpDelete]
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