using HRApi.Data;
using HRApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace HRApi.Controllers
{
    public static class LeaveRequestStatus
    {
        public const string Pending = "Chờ duyệt";
        public const string Approved = "Đã duyệt";
        public const string Rejected = "Từ chối";
    }

    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DonNghiPhepController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public DonNghiPhepController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // --- HÀM HỖ TRỢ ---
        private string ConvertToUnSign(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            input = input.Trim().ToLower();
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = input.Normalize(NormalizationForm.FormD);
            string result = regex.Replace(temp, string.Empty).Replace('\u0111', 'd').Replace('\u0110', 'd');
            return result;
        }

        public class DonNghiPhepCreateDto
        {
            [Required]
            public DateTime NgayBatDau { get; set; }
            [Required]
            public DateTime NgayKetThuc { get; set; }
            [Required]
            public double SoNgayNghi { get; set; }
            [Required]
            public string LyDo { get; set; }
            public IFormFile? File { get; set; }
        }

        // POST: api/DonNghiPhep/create-with-file
        [HttpPost("create-with-file")]
        public async Task<IActionResult> CreateDonNghiPhep([FromForm] DonNghiPhepCreateDto dto)
        {
            var maNhanVien = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (maNhanVien == null) return Unauthorized();

            if (dto.NgayBatDau.Date < DateTime.Today) return BadRequest(new { message = "Không thể đăng ký nghỉ cho ngày quá khứ." });
            if (dto.NgayKetThuc < dto.NgayBatDau) return BadRequest(new { message = "Ngày kết thúc không hợp lệ." });

            // Logic check sơ bộ (chỉ mang tính cảnh báo, việc trừ chính xác sẽ nằm ở bước Duyệt)
            if (dto.LyDo.Contains("Nghỉ phép năm"))
            {
                var currentYear = DateTime.Now.Year;
                var paidLeaveDaysTakenThisYear = await _context.ChamCongs
                    .CountAsync(c => c.MaNhanVien == maNhanVien &&
                                     c.NgayChamCong.Year == currentYear &&
                                     c.NgayCong == 1.0 &&
                                     !string.IsNullOrEmpty(c.GhiChu) &&
                                     (c.GhiChu.Contains("Nghỉ phép") || c.GhiChu.Contains("Nghỉ có phép"))); // Chỉ đếm nghỉ phép, KHÔNG đếm công tác

                var remainingLeaveDays = 12 - paidLeaveDaysTakenThisYear;
                if (remainingLeaveDays <= 0)
                {
                    // Vẫn cho tạo đơn nhưng cảnh báo (hoặc chặn tùy nghiệp vụ)
                    // Ở đây ta cứ để tạo, khi duyệt sẽ chuyển thành Không lương
                }
            }

            string? filePath = null;
            if (dto.File != null)
            {
                var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "donnghi");
                if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);
                var fileName = $"{Guid.NewGuid()}_{dto.File.FileName}";
                filePath = Path.Combine(uploadsDir, fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.File.CopyToAsync(fileStream);
                }
                filePath = $"/uploads/donnghi/{fileName}";
            }

            var donNghiPhep = new DonNghiPhep
            {
                MaNhanVien = maNhanVien,
                NgayBatDau = dto.NgayBatDau.Date,
                NgayKetThuc = dto.NgayKetThuc.Date,
                SoNgayNghi = dto.SoNgayNghi,
                LyDo = dto.LyDo,
                TepDinhKem = filePath,
                TrangThai = LeaveRequestStatus.Pending,
                NgayGuiDon = DateTime.Now
            };

            _context.DonNghiPheps.Add(donNghiPhep);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Gửi đơn thành công!" });
        }

        // GET: api/DonNghiPhep
        [HttpGet]
        [Authorize(Roles = "Trưởng phòng,Kế toán trưởng,Giám đốc,Tổng giám đốc,Nhân sự trưởng")]
        public async Task<ActionResult<IEnumerable<object>>> GetAllRequests(
            [FromQuery] string? trangThai,
            [FromQuery] string? maPhongBan,
            [FromQuery] string? searchTerm,
            [FromQuery] DateTime? tuNgay,
            [FromQuery] DateTime? denNgay)
        {
            var currentUserRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role")?.Value;
            var currentUserMaPhongBan = User.Claims.FirstOrDefault(c => c.Type == "MaPhongBan")?.Value;

            var query = _context.DonNghiPheps
                .Include(d => d.NhanVien).ThenInclude(nv => nv.PhongBan)
                .AsQueryable();

            // --- PHÂN QUYỀN ---
            if (currentUserRole == "Trưởng phòng")
            {
                if (!string.IsNullOrEmpty(currentUserMaPhongBan))
                    query = query.Where(d => d.NhanVien.MaPhongBan == currentUserMaPhongBan);
                else
                    return Ok(new List<object>());
            }

            // --- BỘ LỌC ---
            if (!string.IsNullOrEmpty(trangThai)) query = query.Where(d => d.TrangThai == trangThai);
            if (!string.IsNullOrEmpty(maPhongBan)) query = query.Where(d => d.NhanVien.MaPhongBan == maPhongBan);
            if (!string.IsNullOrEmpty(searchTerm))
            {
                var lowerSearch = searchTerm.ToLower();
                query = query.Where(d => d.NhanVien.HoTen.ToLower().Contains(lowerSearch) || d.MaNhanVien.ToLower().Contains(lowerSearch));
            }
            if (tuNgay.HasValue) query = query.Where(d => d.NgayBatDau >= tuNgay.Value);
            if (denNgay.HasValue) query = query.Where(d => d.NgayBatDau <= denNgay.Value);

            var requestsData = await query.OrderByDescending(d => d.NgayGuiDon).ToListAsync();

            // --- LOGIC TÍNH PHÉP TỒN CHÍNH XÁC ---
            var empIds = requestsData.Select(r => r.MaNhanVien).Distinct().ToList();

            // Lấy tất cả record chấm công là "Nghỉ phép" của các nhân viên này
            var allLeaveRecords = await _context.ChamCongs
                .Where(c => empIds.Contains(c.MaNhanVien) &&
                            c.NgayCong == 1.0 && // Chỉ tính những ngày được trả lương (Nghỉ có phép)
                            !string.IsNullOrEmpty(c.GhiChu) &&
                            (c.GhiChu.Contains("Nghỉ phép") || c.GhiChu.Contains("Nghỉ có phép"))) // QUAN TRỌNG: Chỉ đếm phép, ko đếm công tác/đi làm
                .Select(c => new { c.MaNhanVien, Year = c.NgayChamCong.Year })
                .ToListAsync();

            var result = requestsData.Select(d =>
            {
                // Tính theo năm của đơn xin nghỉ
                int requestYear = d.NgayBatDau.Year;
                int taken = allLeaveRecords.Count(r => r.MaNhanVien == d.MaNhanVien && r.Year == requestYear);
                int remaining = 12 - taken; // 12 là tổng quỹ phép

                return new
                {
                    d.Id,
                    d.MaNhanVien,
                    HoTenNhanVien = d.NhanVien?.HoTen ?? "N/A",
                    TenPhongBan = d.NhanVien?.PhongBan?.TenPhongBan ?? "N/A",
                    d.NgayBatDau,
                    d.NgayKetThuc,
                    d.SoNgayNghi,
                    d.NgayGuiDon,
                    d.LyDo,
                    d.TepDinhKem,
                    d.TrangThai,
                    RemainingLeaveDays = remaining
                };
            });

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DonNghiPhep>> GetDonNghiPhepById(int id)
        {
            var don = await _context.DonNghiPheps.FindAsync(id);
            if (don == null) return NotFound();
            return don;
        }

        // --- DUYỆT ĐƠN (LOGIC QUAN TRỌNG: TRỪ PHÉP 12 NGÀY) ---
        [HttpPost("approve/{id}")]
        [Authorize(Roles = "Trưởng phòng,Giám đốc,Tổng giám đốc")]
        public async Task<IActionResult> ApproveRequest(int id)
        {
            var request = await _context.DonNghiPheps.Include(d => d.NhanVien).FirstOrDefaultAsync(d => d.Id == id);
            if (request == null || request.TrangThai != LeaveRequestStatus.Pending) return NotFound("Lỗi trạng thái.");

            var currentUserRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role")?.Value;
            var currentUserMaPhongBan = User.Claims.FirstOrDefault(c => c.Type == "MaPhongBan")?.Value;

            if (currentUserRole == "Trưởng phòng")
            {
                if (request.NhanVien?.MaPhongBan != currentUserMaPhongBan) return Forbid("Không đúng phòng ban.");
            }

            // 1. Đếm số ngày phép ĐÃ DÙNG trước khi duyệt đơn này
            // Chỉ đếm "Nghỉ phép", KHÔNG đếm "Công tác"
            var requestYear = request.NgayBatDau.Year;
            var currentUsedDays = await _context.ChamCongs.CountAsync(c =>
                    c.MaNhanVien == request.MaNhanVien &&
                    c.NgayChamCong.Year == requestYear &&
                    c.NgayCong == 1.0 &&
                    !string.IsNullOrEmpty(c.GhiChu) &&
                    (c.GhiChu.Contains("Nghỉ phép") || c.GhiChu.Contains("Nghỉ có phép")));

            // 2. Duyệt từng ngày trong đơn
            for (var date = request.NgayBatDau; date.Date <= request.NgayKetThuc.Date; date = date.AddDays(1))
            {
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday) continue;

                double ngayCongValue = 0.0;
                string ghiChuMoi = "";

                // CHECK QUOTA: Nếu đã dùng < 12 ngày -> Được tính phép (Công=1.0) -> Tăng biến đếm
                if (currentUsedDays < 12)
                {
                    ngayCongValue = 1.0;
                    ghiChuMoi = $"Nghỉ có phép: {request.LyDo}";
                    currentUsedDays++; // Tăng lên để ngày tiếp theo trong vòng lặp biết là đã dùng
                }
                else
                {
                    // Đã hết 12 ngày -> Tính không lương (Công=0.0)
                    ngayCongValue = 0.0;
                    ghiChuMoi = $"Nghỉ không phép (hết quota): {request.LyDo}";
                }

                // Cập nhật vào bảng chấm công
                var existing = await _context.ChamCongs.FirstOrDefaultAsync(c => c.MaNhanVien == request.MaNhanVien && c.NgayChamCong.Date == date.Date);

                if (existing != null)
                {
                    existing.NgayCong = ngayCongValue;
                    existing.GhiChu = ghiChuMoi;
                    existing.GioCheckOut = null; // Xóa giờ check-in/out nếu có (vì đã nghỉ)
                }
                else
                {
                    _context.ChamCongs.Add(new ChamCong
                    {
                        MaNhanVien = request.MaNhanVien,
                        NgayChamCong = date.Date,
                        NgayCong = ngayCongValue,
                        GhiChu = ghiChuMoi
                    });
                }
            }

            request.TrangThai = LeaveRequestStatus.Approved;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã duyệt đơn." });
        }

        // --- TỪ CHỐI ĐƠN ---
        [HttpPost("reject/{id}")]
        [Authorize(Roles = "Trưởng phòng,Giám đốc,Tổng giám đốc")]
        public async Task<IActionResult> RejectRequest(int id)
        {
            var request = await _context.DonNghiPheps.Include(d => d.NhanVien).FirstOrDefaultAsync(d => d.Id == id);
            if (request == null || request.TrangThai != LeaveRequestStatus.Pending) return NotFound();

            var currentUserRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role")?.Value;
            var currentUserMaPhongBan = User.Claims.FirstOrDefault(c => c.Type == "MaPhongBan")?.Value;

            if (currentUserRole == "Trưởng phòng" && request.NhanVien?.MaPhongBan != currentUserMaPhongBan)
                return Forbid();

            request.TrangThai = LeaveRequestStatus.Rejected;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã từ chối." });
        }

        // GET: api/DonNghiPhep/balance
        // API này dùng để lấy số phép còn lại của User đang đăng nhập
        [HttpGet("balance")]
        public async Task<IActionResult> GetMyLeaveBalance()
        {
            var maNhanVien = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (maNhanVien == null) return Unauthorized();

            var currentYear = DateTime.Now.Year;

            // Đếm số ngày đã nghỉ phép trong năm nay (dựa vào bảng chấm công)
            var paidLeaveDaysTaken = await _context.ChamCongs
                .CountAsync(c => c.MaNhanVien == maNhanVien &&
                                 c.NgayChamCong.Year == currentYear &&
                                 c.NgayCong == 1.0 &&
                                 !string.IsNullOrEmpty(c.GhiChu) &&
                                 (c.GhiChu.Contains("Nghỉ phép") || c.GhiChu.Contains("Nghỉ có phép")));

            var remaining = 12 - paidLeaveDaysTaken;

            return Ok(new { remainingDays = remaining });
        }
    }

}