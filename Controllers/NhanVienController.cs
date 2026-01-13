using HRApi.Data;
using HRApi.DTOs;
using HRApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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

        #region Get Methods
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NhanVienDetailDto>>> GetNhanViens(
           [FromQuery] string? maPhongBan,
           [FromQuery] string? searchTerm,
           [FromQuery] string? maTrinhDoHocVan,
           [FromQuery] string? maChucVuNV,
           [FromQuery] bool? TrangThai)
        {
            var currentUserRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var currentUserMaPhongBan = User.Claims.FirstOrDefault(c => c.Type == "MaPhongBan")?.Value;

            var query = _context.NhanViens.AsQueryable();

            // --- LOGIC PHÂN QUYỀN MỚI ---
            if (currentUserRole == "Trường phòng")
            {
                if (!string.IsNullOrEmpty(currentUserMaPhongBan))
                    query = query.Where(nv => nv.MaPhongBan == currentUserMaPhongBan);
                else
                    return Ok(new List<NhanVienDetailDto>());
            }
            else if (currentUserRole == "Kế toán trưởng" || currentUserRole == "Giám đốc" || currentUserRole == "Tổng giám đốc")
            {
                // Full access
            }
            else if (currentUserRole == "Nhân viên")
            {
                return Forbid("Bạn không có quyền truy cập vào tài nguyên này.");
            }

            if (!string.IsNullOrEmpty(maPhongBan)) query = query.Where(x => x.MaPhongBan == maPhongBan);
            if (!string.IsNullOrEmpty(maChucVuNV)) query = query.Where(x => x.MaChucVuNV == maChucVuNV);
            if (!string.IsNullOrEmpty(maTrinhDoHocVan)) query = query.Where(x => x.MaTrinhDoHocVan == maTrinhDoHocVan);
            if (TrangThai.HasValue) query = query.Where(x => x.TrangThai == TrangThai.Value);

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
                .Include(nv => nv.QuanLyTrucTiep)
                .Include(nv => nv.UserRole)
                .Select(nv => new NhanVienDetailDto
                {
                    MaNhanVien = nv.MaNhanVien,
                    HoTen = nv.HoTen,
                    TrangThai = nv.TrangThai,
                    HinhAnh = nv.HinhAnh,
                    NgaySinh = nv.NgaySinh.HasValue ? nv.NgaySinh.Value.Date : (DateTime?)null,
                    GioiTinh = nv.GioiTinh,
                    DanToc = nv.DanToc,
                    TonGiao = nv.TonGiao,
                    QueQuan = nv.QueQuan,
                    NoiSinh = nv.NoiSinh,
                    QuocTich = nv.QuocTich,
                    TinhTrangHonNhan = nv.TinhTrangHonNhan,
                    CCCD = nv.CCCD,
                    NgayCapCCCD = nv.NgayCapCCCD.HasValue ? nv.NgayCapCCCD.Value.Date : (DateTime?)null,
                    NoiCapCCCD = nv.NoiCapCCCD,
                    NgayHetHanCCCD = nv.NgayHetHanCCCD,
                    SoHoChieu = nv.SoHoChieu,
                    NgayCapHoChieu = nv.NgayCapHoChieu,
                    NgayHetHanHoChieu = nv.NgayHetHanHoChieu,
                    NoiCapHoChieu = nv.NoiCapHoChieu,
                    Email = nv.Email,
                    sdt_NhanVien = nv.sdt_NhanVien,
                    NguoiLienHeKhanCap = nv.NguoiLienHeKhanCap,
                    SdtKhanCap = nv.SdtKhanCap,
                    QuanHeKhanCap = nv.QuanHeKhanCap,
                    DiaChiKhanCap = nv.DiaChiKhanCap,
                    DiaChiThuongTru = nv.DiaChiThuongTru,
                    PhuongXaThuongTru = nv.PhuongXaThuongTru,
                    QuanHuyenThuongTru = nv.QuanHuyenThuongTru,
                    TinhThanhThuongTru = nv.TinhThanhThuongTru,
                    QuocGiaThuongTru = nv.QuocGiaThuongTru,
                    DiaChiTamTru = nv.DiaChiTamTru,
                    NgayVaoLam = nv.NgayVaoLam,
                    NgayNghiViec = nv.NgayNghiViec,
                    LoaiNhanVien = nv.LoaiNhanVien,
                    MaQuanLyTrucTiep = nv.MaQuanLyTrucTiep,
                    TenQuanLyTrucTiep = nv.QuanLyTrucTiep != null ? nv.QuanLyTrucTiep.HoTen : null,
                    MaPhongBan = nv.MaPhongBan,
                    MaChucVuNV = nv.MaChucVuNV,
                    RoleId = nv.RoleId,
                    MaTrinhDoHocVan = nv.MaTrinhDoHocVan,
                    MaChuyenNganh = nv.MaChuyenNganh,
                    NoiDaoTao = nv.NoiDaoTao,
                    HeDaoTao = nv.HeDaoTao,
                    ChuyenNganhChiTiet = nv.ChuyenNganhChiTiet,
                    SoTaiKhoanNH = nv.SoTaiKhoanNH,
                    TenNganHang = nv.TenNganHang,
                    TenTaiKhoanNH = nv.TenTaiKhoanNH,
                    SoBHYT = nv.SoBHYT,
                    SoBHXH = nv.SoBHXH,
                    NoiDKKCB = nv.NoiDKKCB,
                    TenPhongBan = nv.PhongBan != null ? nv.PhongBan.TenPhongBan : null,
                    TenChucVu = nv.ChucVuNhanVien != null ? nv.ChucVuNhanVien.TenChucVu : null,
                    TenChuyenNganh = nv.ChuyenNganh != null ? nv.ChuyenNganh.TenChuyenNganh : null,
                    TenTrinhDoHocVan = nv.TrinhDoHocVan != null ? nv.TrinhDoHocVan.TenTrinhDo : null,
                    TenRole = nv.UserRole != null ? nv.UserRole.NameRole : null,
                    LuongCoBan = nv.LuongCoBan,
                    LuongTroCap = nv.LuongTroCap,
                    SoHopDong = nv.SoHopDong
                })
                .ToListAsync();

            var sortedResult = unsortedResult
                .OrderBy(nv => nv.HoTen?.Contains(" ") == true ? nv.HoTen.Substring(nv.HoTen.LastIndexOf(" ") + 1) : nv.HoTen)
                .ThenBy(nv => nv.HoTen)
                .ToList();

            return Ok(sortedResult);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<NhanVienDetailDto>> GetNhanVien(string id)
        {
            var currentUserRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var currentUserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var currentUserMaPhongBan = User.Claims.FirstOrDefault(c => c.Type == "MaPhongBan")?.Value;

            if (currentUserRole == "Nhân viên" && currentUserId != id)
            {
                return Forbid("Bạn không có quyền truy cập thông tin của nhân viên khác.");
            }

            var query = _context.NhanViens.AsNoTracking().AsQueryable();

            if (currentUserRole == "Trưởng phòng" && !string.IsNullOrEmpty(currentUserMaPhongBan))
            {
                query = query.Where(nv => nv.MaPhongBan == currentUserMaPhongBan);
            }

            var nhanVien = await query
                .Include(nv => nv.PhongBan)
                .Include(nv => nv.ChucVuNhanVien)
                .Include(nv => nv.ChuyenNganh)
                .Include(nv => nv.TrinhDoHocVan)
                .Include(nv => nv.QuanLyTrucTiep)
                .Include(nv => nv.UserRole)
                .Where(nv => nv.MaNhanVien == id)
                .Select(nv => new NhanVienDetailDto
                {
                    MaNhanVien = nv.MaNhanVien,
                    HoTen = nv.HoTen,
                    TrangThai = nv.TrangThai,
                    HinhAnh = nv.HinhAnh,
                    NgaySinh = nv.NgaySinh,
                    GioiTinh = nv.GioiTinh,
                    DanToc = nv.DanToc,
                    TonGiao = nv.TonGiao,
                    QueQuan = nv.QueQuan,
                    NoiSinh = nv.NoiSinh,
                    QuocTich = nv.QuocTich,
                    TinhTrangHonNhan = nv.TinhTrangHonNhan,
                    CCCD = nv.CCCD,
                    NgayCapCCCD = nv.NgayCapCCCD,
                    NoiCapCCCD = nv.NoiCapCCCD,
                    NgayHetHanCCCD = nv.NgayHetHanCCCD,
                    SoHoChieu = nv.SoHoChieu,
                    NgayCapHoChieu = nv.NgayCapHoChieu,
                    NgayHetHanHoChieu = nv.NgayHetHanHoChieu,
                    NoiCapHoChieu = nv.NoiCapHoChieu,
                    Email = nv.Email,
                    sdt_NhanVien = nv.sdt_NhanVien,
                    NguoiLienHeKhanCap = nv.NguoiLienHeKhanCap,
                    SdtKhanCap = nv.SdtKhanCap,
                    QuanHeKhanCap = nv.QuanHeKhanCap,
                    DiaChiKhanCap = nv.DiaChiKhanCap,
                    DiaChiThuongTru = nv.DiaChiThuongTru,
                    PhuongXaThuongTru = nv.PhuongXaThuongTru,
                    QuanHuyenThuongTru = nv.QuanHuyenThuongTru,
                    TinhThanhThuongTru = nv.TinhThanhThuongTru,
                    QuocGiaThuongTru = nv.QuocGiaThuongTru,
                    DiaChiTamTru = nv.DiaChiTamTru,
                    NgayVaoLam = nv.NgayVaoLam,
                    NgayNghiViec = nv.NgayNghiViec,
                    LoaiNhanVien = nv.LoaiNhanVien,
                    MaQuanLyTrucTiep = nv.MaQuanLyTrucTiep,
                    TenQuanLyTrucTiep = nv.QuanLyTrucTiep != null ? nv.QuanLyTrucTiep.HoTen : null,
                    MaPhongBan = nv.MaPhongBan,
                    MaChucVuNV = nv.MaChucVuNV,
                    RoleId = nv.RoleId,
                    MaTrinhDoHocVan = nv.MaTrinhDoHocVan,
                    MaChuyenNganh = nv.MaChuyenNganh,
                    NoiDaoTao = nv.NoiDaoTao,
                    HeDaoTao = nv.HeDaoTao,
                    ChuyenNganhChiTiet = nv.ChuyenNganhChiTiet,
                    SoTaiKhoanNH = nv.SoTaiKhoanNH,
                    TenNganHang = nv.TenNganHang,
                    TenTaiKhoanNH = nv.TenTaiKhoanNH,
                    SoBHYT = nv.SoBHYT,
                    SoBHXH = nv.SoBHXH,
                    NoiDKKCB = nv.NoiDKKCB,
                    LuongCoBan = nv.LuongCoBan,
                    LuongTroCap = nv.LuongTroCap,
                    SoHopDong = nv.SoHopDong
                })
                .FirstOrDefaultAsync();

            if (nhanVien == null) return NotFound("Không tìm thấy nhân viên hoặc bạn không có quyền truy cập.");
            return Ok(nhanVien);
        }

        [Authorize]
        [HttpGet("managers")]
        public async Task<ActionResult<IEnumerable<QuanLySelectDto>>> GetManagers([FromQuery] string? excludeId)
        {
            var query = _context.NhanViens
                .AsNoTracking()
                .Where(nv => nv.TrangThai == true)
                .AsQueryable();

            if (!string.IsNullOrEmpty(excludeId))
            {
                query = query.Where(nv => nv.MaNhanVien != excludeId);
            }

            var managers = await query
                .Include(nv => nv.ChucVuNhanVien)
                .Select(nv => new QuanLySelectDto
                {
                    MaNhanVien = nv.MaNhanVien,
                    HoTen = nv.HoTen,
                    TenChucVu = nv.ChucVuNhanVien != null ? nv.ChucVuNhanVien.TenChucVu : "Chưa có chức vụ",
                    MaPhongBan = nv.MaPhongBan
                })
                .OrderBy(m => m.HoTen)
                .ToListAsync();

            return Ok(managers);
        }
        #endregion

        #region Create & Update
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<NhanVien>> CreateNhanVien([FromBody] NhanVienCreateUpdateDto dto)
        {
            if (dto == null) return BadRequest("Dữ liệu không hợp lệ.");
            if (string.IsNullOrEmpty(dto.MatKhau)) return BadRequest("Mật khẩu là bắt buộc.");
            if (string.IsNullOrEmpty(dto.MaChucVuNV)) return BadRequest("Chức vụ là bắt buộc để gán quyền.");

            int? assignedRoleId = dto.RoleId;
            if (!assignedRoleId.HasValue || assignedRoleId == 0)
            {
                assignedRoleId = await GetRoleIdFromChucVuAsync(dto.MaChucVuNV);
            }

            if (assignedRoleId == null)
            {
                return BadRequest($"Chức vụ '{dto.MaChucVuNV}' không có vai trò tương ứng trong hệ thống.");
            }

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

            var newNhanVien = new NhanVien
            {
                MaNhanVien = newMaNV,
                MatKhau = BCrypt.Net.BCrypt.HashPassword(dto.MatKhau),
                TrangThai = true,
                HinhAnh = dto.HinhAnh,
                HoTen = dto.HoTen,
                NgaySinh = dto.NgaySinh,
                GioiTinh = dto.GioiTinh,
                DanToc = dto.DanToc,
                TonGiao = dto.TonGiao,
                QueQuan = dto.QueQuan,
                NoiSinh = dto.NoiSinh,
                QuocTich = dto.QuocTich,
                TinhTrangHonNhan = dto.TinhTrangHonNhan,
                CCCD = dto.CCCD,
                NgayCapCCCD = dto.NgayCapCCCD,
                NoiCapCCCD = dto.NoiCapCCCD,
                NgayHetHanCCCD = dto.NgayHetHanCCCD,
                SoHoChieu = dto.SoHoChieu,
                NgayCapHoChieu = dto.NgayCapHoChieu,
                NgayHetHanHoChieu = dto.NgayHetHanHoChieu,
                NoiCapHoChieu = dto.NoiCapHoChieu,
                Email = dto.Email,
                sdt_NhanVien = dto.sdt_NhanVien,
                NguoiLienHeKhanCap = dto.NguoiLienHeKhanCap,
                SdtKhanCap = dto.SdtKhanCap,
                QuanHeKhanCap = dto.QuanHeKhanCap,
                DiaChiKhanCap = dto.DiaChiKhanCap,
                DiaChiThuongTru = dto.DiaChiThuongTru,
                PhuongXaThuongTru = dto.PhuongXaThuongTru,
                QuanHuyenThuongTru = dto.QuanHuyenThuongTru,
                TinhThanhThuongTru = dto.TinhThanhThuongTru,
                QuocGiaThuongTru = dto.QuocGiaThuongTru,
                DiaChiTamTru = dto.DiaChiTamTru,
                NgayVaoLam = dto.NgayVaoLam,
                NgayNghiViec = dto.NgayNghiViec,
                LoaiNhanVien = dto.LoaiNhanVien,
                MaQuanLyTrucTiep = dto.MaQuanLyTrucTiep,
                MaPhongBan = dto.MaPhongBan,
                MaChucVuNV = dto.MaChucVuNV,
                RoleId = assignedRoleId.Value,
                MaTrinhDoHocVan = dto.MaTrinhDoHocVan,
                MaChuyenNganh = dto.MaChuyenNganh,
                NoiDaoTao = dto.NoiDaoTao,
                HeDaoTao = dto.HeDaoTao,
                ChuyenNganhChiTiet = dto.ChuyenNganhChiTiet,
                TenNganHang = dto.TenNganHang,
                SoTaiKhoanNH = dto.SoTaiKhoanNH,
                TenTaiKhoanNH = dto.TenTaiKhoanNH,
                SoBHYT = dto.SoBHYT,
                SoBHXH = dto.SoBHXH,
                NoiDKKCB = dto.NoiDKKCB,
                LuongCoBan = dto.LuongCoBan,
                LuongTroCap = dto.LuongTroCap,
                SoHopDong = dto.SoHopDong
            };

            _context.NhanViens.Add(newNhanVien);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetNhanVien), new { id = newNhanVien.MaNhanVien }, newNhanVien);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNhanVien(string id, [FromBody] NhanVienCreateUpdateDto dto)
        {
            var existingNhanVien = await _context.NhanViens.FindAsync(id);
            if (existingNhanVien == null) return NotFound("Không tìm thấy nhân viên.");

            if (!string.IsNullOrEmpty(dto.MaChucVuNV) && existingNhanVien.MaChucVuNV != dto.MaChucVuNV)
            {
                int? newRoleId = await GetRoleIdFromChucVuAsync(dto.MaChucVuNV);
                if (newRoleId.HasValue) existingNhanVien.RoleId = newRoleId.Value;
            }

            existingNhanVien.HoTen = dto.HoTen;
            existingNhanVien.NgaySinh = dto.NgaySinh;
            existingNhanVien.GioiTinh = dto.GioiTinh;
            existingNhanVien.DanToc = dto.DanToc;
            existingNhanVien.TonGiao = dto.TonGiao;
            existingNhanVien.QueQuan = dto.QueQuan;
            existingNhanVien.NoiSinh = dto.NoiSinh;
            existingNhanVien.QuocTich = dto.QuocTich;
            existingNhanVien.TinhTrangHonNhan = dto.TinhTrangHonNhan;
            existingNhanVien.CCCD = dto.CCCD;
            existingNhanVien.NgayCapCCCD = dto.NgayCapCCCD;
            existingNhanVien.NoiCapCCCD = dto.NoiCapCCCD;
            existingNhanVien.NgayHetHanCCCD = dto.NgayHetHanCCCD;
            existingNhanVien.SoHoChieu = dto.SoHoChieu;
            existingNhanVien.NgayCapHoChieu = dto.NgayCapHoChieu;
            existingNhanVien.NgayHetHanHoChieu = dto.NgayHetHanHoChieu;
            existingNhanVien.NoiCapHoChieu = dto.NoiCapHoChieu;
            existingNhanVien.Email = dto.Email;
            existingNhanVien.sdt_NhanVien = dto.sdt_NhanVien;
            existingNhanVien.NguoiLienHeKhanCap = dto.NguoiLienHeKhanCap;
            existingNhanVien.SdtKhanCap = dto.SdtKhanCap;
            existingNhanVien.QuanHeKhanCap = dto.QuanHeKhanCap;
            existingNhanVien.DiaChiKhanCap = dto.DiaChiKhanCap;
            existingNhanVien.DiaChiThuongTru = dto.DiaChiThuongTru;
            existingNhanVien.PhuongXaThuongTru = dto.PhuongXaThuongTru;
            existingNhanVien.QuanHuyenThuongTru = dto.QuanHuyenThuongTru;
            existingNhanVien.TinhThanhThuongTru = dto.TinhThanhThuongTru;
            existingNhanVien.QuocGiaThuongTru = dto.QuocGiaThuongTru;
            existingNhanVien.DiaChiTamTru = dto.DiaChiTamTru;
            existingNhanVien.HinhAnh = dto.HinhAnh;
            existingNhanVien.NgayVaoLam = dto.NgayVaoLam;
            existingNhanVien.NgayNghiViec = dto.NgayNghiViec;
            existingNhanVien.LoaiNhanVien = dto.LoaiNhanVien;
            existingNhanVien.MaQuanLyTrucTiep = dto.MaQuanLyTrucTiep;
            existingNhanVien.MaPhongBan = dto.MaPhongBan;
            existingNhanVien.MaChucVuNV = dto.MaChucVuNV;
            existingNhanVien.TrangThai = dto.TrangThai;
            existingNhanVien.MaTrinhDoHocVan = dto.MaTrinhDoHocVan;
            existingNhanVien.MaChuyenNganh = dto.MaChuyenNganh;
            existingNhanVien.NoiDaoTao = dto.NoiDaoTao;
            existingNhanVien.HeDaoTao = dto.HeDaoTao;
            existingNhanVien.ChuyenNganhChiTiet = dto.ChuyenNganhChiTiet;
            existingNhanVien.TenNganHang = dto.TenNganHang;
            existingNhanVien.SoTaiKhoanNH = dto.SoTaiKhoanNH;
            existingNhanVien.TenTaiKhoanNH = dto.TenTaiKhoanNH;
            existingNhanVien.SoBHYT = dto.SoBHYT;
            existingNhanVien.SoBHXH = dto.SoBHXH;
            existingNhanVien.NoiDKKCB = dto.NoiDKKCB;
            existingNhanVien.LuongCoBan = dto.LuongCoBan;
            existingNhanVien.LuongTroCap = dto.LuongTroCap;
            existingNhanVien.SoHopDong = dto.SoHopDong;

            if (!string.IsNullOrEmpty(dto.MatKhau))
            {
                existingNhanVien.MatKhau = BCrypt.Net.BCrypt.HashPassword(dto.MatKhau);
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        private async Task<int?> GetRoleIdFromChucVuAsync(string maChucVuNV)
        {
            var chucVu = await _context.ChucVuNhanViens.AsNoTracking()
                .FirstOrDefaultAsync(cv => cv.MaChucVuNV == maChucVuNV);
            return chucVu?.RoleId;
        }
        #endregion

        #region Other Methods
        [Authorize]
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
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Nhân viên {nhanVien.HoTen} đã được vô hiệu hóa." });
        }

        [Authorize]
        [HttpPost("{id}/activate")]
        public async Task<IActionResult> ActivateNhanVien(string id)
        {
            var nhanVien = await _context.NhanViens.FindAsync(id);
            if (nhanVien == null) return NotFound("Không tìm thấy nhân viên.");
            nhanVien.TrangThai = true;
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Nhân viên {nhanVien.HoTen} đã được kích hoạt lại." });
        }

        [Authorize]
        [HttpPost("import")]
        public async Task<IActionResult> ImportNhanViens([FromBody] List<NhanVienCreateUpdateDto> dtos)
        {
            if (dtos == null || !dtos.Any()) return BadRequest("Không có dữ liệu.");

            var allMaNVs = await _context.NhanViens.Select(nv => nv.MaNhanVien).ToListAsync();
            int maxId = 0;
            if (allMaNVs.Any())
            {
                maxId = allMaNVs
                    .Where(ma => ma != null && ma.Length > 2 && ma.StartsWith("NV"))
                    .Select(ma => int.TryParse(ma.AsSpan(2), out var id) ? id : 0)
                    .DefaultIfEmpty(0).Max();
            }

            var newNhanViens = new List<NhanVien>();
            var errors = new List<string>();

            foreach (var dto in dtos)
            {
                if (string.IsNullOrEmpty(dto.MaChucVuNV) || string.IsNullOrEmpty(dto.HoTen))
                {
                    errors.Add($"Bỏ qua: {dto.HoTen ?? "Không tên"} (Thiếu Họ Tên/Chức Vụ).");
                    continue;
                }

                int? assignedRoleId = dto.RoleId ?? await GetRoleIdFromChucVuAsync(dto.MaChucVuNV);
                if (assignedRoleId == null)
                {
                    errors.Add($"Bỏ qua: {dto.HoTen} (Lỗi RoleId).");
                    continue;
                }

                maxId++;
                string newMaNV = $"NV{(maxId):D4}";
                string password = string.IsNullOrEmpty(dto.MatKhau) ? "123456" : dto.MatKhau;

                var newNhanVien = new NhanVien
                {
                    MaNhanVien = newMaNV,
                    MatKhau = BCrypt.Net.BCrypt.HashPassword(password),
                    TrangThai = dto.TrangThai,
                    HoTen = dto.HoTen,
                    Email = dto.Email,
                    MaChucVuNV = dto.MaChucVuNV,
                    MaPhongBan = dto.MaPhongBan,
                    RoleId = assignedRoleId.Value,
                    LuongCoBan = dto.LuongCoBan,
                    SoHopDong = dto.SoHopDong
                };
                newNhanViens.Add(newNhanVien);
            }

            if (!newNhanViens.Any()) return BadRequest(new { message = "Không thêm được nhân viên nào.", details = errors });

            try
            {
                await _context.NhanViens.AddRangeAsync(newNhanViens);
                await _context.SaveChangesAsync();
                return Ok(new { message = $"Đã nhập {newNhanViens.Count} nhân viên.", details = errors });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi: {ex.Message}");
            }
        }
        #endregion
    }
}