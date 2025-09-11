using HRApi.Data;
using HRApi.DTOs;
using HRApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace HRApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NhanVienController : ControllerBase
    {
        private readonly AppDbContext _context;
        public NhanVienController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NhanVienDetailDto>>> GetNhanViens(
     [FromQuery] string? maPhongBan,
     [FromQuery] string? searchTerm,
     [FromQuery] string? maTrinhDoHocVan,
     [FromQuery] string? maChucVuNV,
     [FromQuery] bool? TrangThai)
        {
            var query = _context.NhanViens.AsQueryable();

            if (!string.IsNullOrEmpty(maPhongBan))
            {
                query = query.Where(x => x.MaPhongBan == maPhongBan);
            }

            if (!string.IsNullOrEmpty(maChucVuNV))
            {
                query = query.Where(x => x.MaChucVuNV == maChucVuNV);
            }

            if (!string.IsNullOrEmpty(maTrinhDoHocVan))
            {
                query = query.Where(x => x.MaTrinhDoHocVan == maTrinhDoHocVan);
            }

            if (TrangThai.HasValue)
            {
                query = query.Where(x => x.TrangThai == TrangThai.Value);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var lowerCaseSearchTerm = searchTerm.ToLower();
                query = query.Where(x =>
                    (x.HoTen != null && x.HoTen.ToLower().Contains(lowerCaseSearchTerm)) ||
                    (x.MaNhanVien != null && x.MaNhanVien.ToLower().Contains(lowerCaseSearchTerm)) ||
                    (x.Email != null && x.Email.ToLower().Contains(lowerCaseSearchTerm)) ||
                    (x.sdt_NhanVien != null && x.sdt_NhanVien.Contains(searchTerm)) ||
                    (x.CCCD != null && x.CCCD.Contains(searchTerm))
                );
            }

            var unsortedResult = await query.AsNoTracking()
                .Include(nv => nv.PhongBan)
                .Include(nv => nv.ChucVuNhanVien)
                .Include(nv => nv.ChuyenNganh)
                .Include(nv => nv.TrinhDoHocVan)
                .Select(nv => new NhanVienDetailDto
                {
                    MaNhanVien = nv.MaNhanVien,
                    HoTen = nv.HoTen,
                    NgaySinh = nv.NgaySinh.HasValue ? nv.NgaySinh.Value.Date : (DateTime?)null,
                    GioiTinh = nv.GioiTinh,
                    DanToc = nv.DanToc,

                    TinhTrangHonNhan = nv.TinhTrangHonNhan,
                    QueQuan = nv.QueQuan,
                    DiaChiThuongTru = nv.DiaChiThuongTru,
                    DiaChiTamTru = nv.DiaChiTamTru,
                    HinhAnh = nv.HinhAnh,
                    sdt_NhanVien = nv.sdt_NhanVien,
                    Email = nv.Email,
                    CCCD = nv.CCCD,
                    NgayCapCCCD = nv.NgayCapCCCD.HasValue ? nv.NgayCapCCCD.Value.Date : (DateTime?)null,
                    NoiCapCCCD = nv.NoiCapCCCD,
                    LoaiNhanVien = nv.LoaiNhanVien,
                    TrangThai = nv.TrangThai,
                    SoTaiKhoanNH = nv.SoTaiKhoanNH,
                    TenNganHang = nv.TenNganHang,
                    MaPhongBan = nv.MaPhongBan,
                    MaChucVuNV = nv.MaChucVuNV,
                    MaChuyenNganh = nv.MaChuyenNganh,
                    MaTrinhDoHocVan = nv.MaTrinhDoHocVan,
                    TenPhongBan = nv.PhongBan != null ? nv.PhongBan.TenPhongBan : null,
                    TenChucVu = nv.ChucVuNhanVien != null ? nv.ChucVuNhanVien.TenChucVu : null,
                    TenChuyenNganh = nv.ChuyenNganh != null ? nv.ChuyenNganh.TenChuyenNganh : null,
                    TenTrinhDoHocVan = nv.TrinhDoHocVan != null ? nv.TrinhDoHocVan.TenTrinhDo : null
                })
                .ToListAsync();


            var sortedResult = unsortedResult
                .OrderBy(nv => nv.HoTen?.Contains(" ") == true ? nv.HoTen.Substring(nv.HoTen.LastIndexOf(" ") + 1) : nv.HoTen)
                .ThenBy(nv => nv.HoTen)
                .ToList();

            return Ok(sortedResult);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<NhanVienDetailDto>> GetNhanVien(string id)
        {
            var nhanVien = await _context.NhanViens.AsNoTracking()
                .Include(nv => nv.PhongBan)
                .Include(nv => nv.ChucVuNhanVien)
                .Include(nv => nv.ChuyenNganh)
                .Include(nv => nv.TrinhDoHocVan)
                .Include(nv => nv.UserRole)
                .Where(nv => nv.MaNhanVien == id)
                .Select(nv => new NhanVienDetailDto
                {
                    MaNhanVien = nv.MaNhanVien,
                    HoTen = nv.HoTen,
                    NgaySinh = nv.NgaySinh,
                    GioiTinh = nv.GioiTinh,
                    DanToc = nv.DanToc,
                    TinhTrangHonNhan = nv.TinhTrangHonNhan,
                    QueQuan = nv.QueQuan,
                    DiaChiThuongTru = nv.DiaChiThuongTru,
                    DiaChiTamTru = nv.DiaChiTamTru,
                    HinhAnh = nv.HinhAnh,
                    sdt_NhanVien = nv.sdt_NhanVien,
                    Email = nv.Email,
                    CCCD = nv.CCCD,
                    NgayCapCCCD = nv.NgayCapCCCD,
                    NoiCapCCCD = nv.NoiCapCCCD,
                    LoaiNhanVien = nv.LoaiNhanVien,
                    TrangThai = nv.TrangThai,
                    SoTaiKhoanNH = nv.SoTaiKhoanNH,
                    TenNganHang = nv.TenNganHang,
                    MaPhongBan = nv.MaPhongBan,
                    MaChucVuNV = nv.MaChucVuNV,
                    MaChuyenNganh = nv.MaChuyenNganh,
                    MaTrinhDoHocVan = nv.MaTrinhDoHocVan,
                    RoleId = nv.RoleId,
                    TenPhongBan = nv.PhongBan != null ? nv.PhongBan.TenPhongBan : null,
                    TenChucVu = nv.ChucVuNhanVien != null ? nv.ChucVuNhanVien.TenChucVu : null,
                    TenChuyenNganh = nv.ChuyenNganh != null ? nv.ChuyenNganh.TenChuyenNganh : null,
                    TenTrinhDoHocVan = nv.TrinhDoHocVan != null ? nv.TrinhDoHocVan.TenTrinhDo : null,
                    TenRole = nv.UserRole != null ? nv.UserRole.NameRole : null
                })
                .FirstOrDefaultAsync();
            if (nhanVien == null) return NotFound();
            return Ok(nhanVien);
        }

        [HttpPost]
        public async Task<ActionResult<NhanVien>> CreateNhanVien([FromBody] NhanVienCreateUpdateDto dto)
        {
            if (dto == null) return BadRequest("Dữ liệu không hợp lệ.");
            if (string.IsNullOrEmpty(dto.MatKhau)) return BadRequest("Mật khẩu là bắt buộc.");
            if (string.IsNullOrEmpty(dto.MaChucVuNV)) return BadRequest("Chức vụ là bắt buộc để gán quyền.");

            var allMaNVs = await _context.NhanViens.Select(nv => nv.MaNhanVien).ToListAsync();
            int maxId = 0;
            if (allMaNVs.Any())
            {
                maxId = allMaNVs
                    .Where(ma => ma != null && ma.Length > 2 && ma.StartsWith("NV"))
                    .Select(ma => int.TryParse(ma.AsSpan(2), out var id) ? id : 0)
                    .DefaultIfEmpty(0).Max();
            }
            string newMaNV = $"NV{(maxId + 1):D4}";

            int? assignedRoleId = null;
            switch (dto.MaChucVuNV)
            {
                case "GD":
                    var giamDocRole = await _context.UserRoles.FirstOrDefaultAsync(r => r.NameRole == "Giám đốc");
                    if (giamDocRole != null) assignedRoleId = giamDocRole.RoleId;
                    break;
                case "TP":
                    var truongPhongRole = await _context.UserRoles.FirstOrDefaultAsync(r => r.NameRole == "Trưởng phòng");
                    if (truongPhongRole != null) assignedRoleId = truongPhongRole.RoleId;
                    break;
                case "PP":
                    var phoPhongRole = await _context.UserRoles.FirstOrDefaultAsync(r => r.NameRole == "Phó phòng");
                    if (phoPhongRole != null) assignedRoleId = phoPhongRole.RoleId;
                    break;
                case "NS":
                    var nhanSuRole = await _context.UserRoles.FirstOrDefaultAsync(r => r.NameRole == "Nhân viên nhân sự");
                    if (nhanSuRole != null) assignedRoleId = nhanSuRole.RoleId;
                    break;
                default:
                    var nhanVienRole = await _context.UserRoles.FirstOrDefaultAsync(r => r.NameRole == "Nhân viên");
                    if (nhanVienRole != null) assignedRoleId = nhanVienRole.RoleId;
                    break;
            }

            var newNhanVien = new NhanVien
            {
                MaNhanVien = newMaNV,
                MatKhau = BCrypt.Net.BCrypt.HashPassword(dto.MatKhau),
                TrangThai = true,
                HoTen = dto.HoTen,
                NgaySinh = dto.NgaySinh,
                GioiTinh = dto.GioiTinh,
                DanToc = dto.DanToc,
                TinhTrangHonNhan = dto.TinhTrangHonNhan,
                QueQuan = dto.QueQuan,
                DiaChiThuongTru = dto.DiaChiThuongTru,
                DiaChiTamTru = dto.DiaChiTamTru,
                HinhAnh = dto.HinhAnh,
                sdt_NhanVien = dto.sdt_NhanVien,
                Email = dto.Email,
                CCCD = dto.CCCD,
                NgayCapCCCD = dto.NgayCapCCCD,
                NoiCapCCCD = dto.NoiCapCCCD,
                LoaiNhanVien = dto.LoaiNhanVien,
                SoTaiKhoanNH = dto.SoTaiKhoanNH,
                TenNganHang = dto.TenNganHang,
                MaChucVuNV = dto.MaChucVuNV,
                MaPhongBan = dto.MaPhongBan,
                MaChuyenNganh = dto.MaChuyenNganh,
                MaTrinhDoHocVan = dto.MaTrinhDoHocVan,
                RoleId = dto.RoleId
            };
            _context.NhanViens.Add(newNhanVien);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetNhanVien), new { id = newNhanVien.MaNhanVien }, newNhanVien);
        }

        [Authorize(Roles = "Trưởng phòng, Giám đốc")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNhanVien(string id, [FromBody] NhanVienCreateUpdateDto dto)
        {
            var existingNhanVien = await _context.NhanViens.FindAsync(id);
            if (existingNhanVien == null) return NotFound("Không tìm thấy nhân viên.");
            existingNhanVien.HoTen = dto.HoTen;
            existingNhanVien.NgaySinh = dto.NgaySinh;
            existingNhanVien.GioiTinh = dto.GioiTinh;
            existingNhanVien.DanToc = dto.DanToc;
            existingNhanVien.TinhTrangHonNhan = dto.TinhTrangHonNhan;
            existingNhanVien.QueQuan = dto.QueQuan;
            existingNhanVien.DiaChiThuongTru = dto.DiaChiThuongTru;
            existingNhanVien.DiaChiTamTru = dto.DiaChiTamTru;
            existingNhanVien.HinhAnh = dto.HinhAnh;
            existingNhanVien.sdt_NhanVien = dto.sdt_NhanVien;
            existingNhanVien.Email = dto.Email;
            existingNhanVien.CCCD = dto.CCCD;
            existingNhanVien.NgayCapCCCD = dto.NgayCapCCCD;
            existingNhanVien.NoiCapCCCD = dto.NoiCapCCCD;
            existingNhanVien.LoaiNhanVien = dto.LoaiNhanVien;
            existingNhanVien.TrangThai = dto.TrangThai;
            existingNhanVien.SoTaiKhoanNH = dto.SoTaiKhoanNH;
            existingNhanVien.TenNganHang = dto.TenNganHang;
            existingNhanVien.MaChucVuNV = dto.MaChucVuNV;
            existingNhanVien.MaPhongBan = dto.MaPhongBan;
            existingNhanVien.MaChuyenNganh = dto.MaChuyenNganh;
            existingNhanVien.MaTrinhDoHocVan = dto.MaTrinhDoHocVan;
            existingNhanVien.RoleId = dto.RoleId;
            if (!string.IsNullOrEmpty(dto.MatKhau))
            {
                existingNhanVien.MatKhau = BCrypt.Net.BCrypt.HashPassword(dto.MatKhau);
            }
            await _context.SaveChangesAsync();
            return NoContent();
        }
        [HttpPost("UploadImage")]
        public async Task<IActionResult> UploadImage()
        {
            try
            {
                var file = Request.Form.Files.FirstOrDefault();
                if (file == null || file.Length == 0) return BadRequest("Không có file nào được tải lên.");
                var uploadsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                if (!Directory.Exists(uploadsFolderPath)) Directory.CreateDirectory(uploadsFolderPath);
                var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                var filePath = Path.Combine(uploadsFolderPath, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create)) await file.CopyToAsync(stream);
                var publicPath = $"/images/{uniqueFileName}";
                return Ok(new { filePath = publicPath });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DisableNhanVien(string id)
        {
            var nhanVien = await _context.NhanViens.FindAsync(id);
            if (nhanVien == null) return NotFound("Không tìm thấy nhân viên.");
            nhanVien.TrangThai = false;
            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = $"Nhân viên {nhanVien.HoTen} đã được vô hiệu hóa." });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Lỗi khi cập nhật trạng thái: {ex.Message}");
            }
        }
        [HttpPost("{id}/activate")]
        public async Task<IActionResult> ActivateNhanVien(string id)
        {
            var nhanVien = await _context.NhanViens.FindAsync(id);
            if (nhanVien == null) return NotFound("Không tìm thấy nhân viên.");
            nhanVien.TrangThai = true;
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Nhân viên {nhanVien.HoTen} đã được kích hoạt lại." });
        }
    }
}
