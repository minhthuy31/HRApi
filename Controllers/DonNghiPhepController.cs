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

        // --- HÀM HỖ TRỢ: XÓA DẤU TIẾNG VIỆT ---
        private string ConvertToUnSign(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            input = input.Trim().ToLower();
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = input.Normalize(NormalizationForm.FormD);
            string result = regex.Replace(temp, string.Empty).Replace('\u0111', 'd').Replace('\u0110', 'd');
            return result;
        }

        // DTO MỚI: Dùng cho [FromForm]
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

            if (dto.NgayBatDau.Date < DateTime.Today)
            {
                return BadRequest(new { message = "Không thể đăng ký nghỉ cho một ngày trong quá khứ." });
            }
            if (dto.NgayKetThuc < dto.NgayBatDau)
            {
                return BadRequest(new { message = "Ngày kết thúc không thể trước ngày bắt đầu." });
            }

            // Kiểm tra logic số ngày phép còn lại (Giữ nguyên hoặc bỏ tùy bạn, ở đây tôi giữ lại để nhắc nhở user)
            if (dto.LyDo.Contains("Nghỉ phép năm"))
            {
                var startDateOfYear = new DateTime(DateTime.Now.Year, 1, 1);
                var paidLeaveDaysTakenThisYear = await _context.ChamCongs
                    .CountAsync(c => c.MaNhanVien == maNhanVien &&
                                     c.NgayChamCong >= startDateOfYear && c.NgayChamCong.Year == DateTime.Now.Year &&
                                     c.NgayCong == 1.0 && !string.IsNullOrEmpty(c.GhiChu) &&
                                     (c.GhiChu.Contains("Nghỉ phép") || c.GhiChu.Contains("Nghỉ có phép")));

                var remainingLeaveDays = 12 - paidLeaveDaysTakenThisYear;
                if (dto.SoNgayNghi > remainingLeaveDays)
                {
                    return BadRequest(new { message = $"Bạn chỉ còn {remainingLeaveDays} ngày phép. Không thể xin nghỉ {dto.SoNgayNghi} ngày." });
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

            return Ok(new { message = "Gửi đơn nghỉ phép thành công!" });
        }

        // GET: api/DonNghiPhep
        [HttpGet]
        [Authorize(Roles = "Nhân sự phòng,Trưởng phòng,Nhân sự tổng,Giám đốc")]
        public async Task<ActionResult<IEnumerable<object>>> GetAllRequests([FromQuery] string? trangThai)
        {
            var currentUserRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role")?.Value;
            var currentUserMaPhongBan = User.Claims.FirstOrDefault(c => c.Type == "MaPhongBan")?.Value;
            var roleClean = ConvertToUnSign(currentUserRole);

            var query = _context.DonNghiPheps.Include(d => d.NhanVien).AsQueryable();

            if (roleClean == "truong phong" || roleClean == "nhan su phong")
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

            if (!string.IsNullOrEmpty(trangThai))
            {
                query = query.Where(d => d.TrangThai == trangThai);
            }

            // ***** CẬP NHẬT: TÍNH SỐ NGÀY PHÉP CÒN LẠI ĐỂ HIỂN THỊ *****
            var currentYear = DateTime.Now.Year;

            // Lấy danh sách đơn
            var requestsData = await query.OrderByDescending(d => d.NgayGuiDon).ToListAsync();

            // Lấy danh sách ID nhân viên cần tính
            var empIds = requestsData.Select(r => r.MaNhanVien).Distinct().ToList();

            // Tính số ngày phép đã nghỉ của từng nhân viên trong năm nay
            var paidLeaveStats = await _context.ChamCongs
                .Where(c => empIds.Contains(c.MaNhanVien) &&
                            c.NgayChamCong.Year == currentYear &&
                            c.NgayCong == 1.0 &&
                            !string.IsNullOrEmpty(c.GhiChu) &&
                            (c.GhiChu.Contains("Nghỉ phép") || c.GhiChu.Contains("Nghỉ có phép")))
                .GroupBy(c => c.MaNhanVien)
                .Select(g => new { MaNhanVien = g.Key, Taken = g.Count() })
                .ToDictionaryAsync(x => x.MaNhanVien, x => x.Taken);

            // Map lại dữ liệu trả về
            var result = requestsData.Select(d =>
            {
                int taken = paidLeaveStats.ContainsKey(d.MaNhanVien) ? paidLeaveStats[d.MaNhanVien] : 0;
                int remaining = 12 - taken; // Giả sử quỹ phép là 12

                return new
                {
                    Id = d.Id,
                    MaNhanVien = d.MaNhanVien,
                    HoTenNhanVien = d.NhanVien != null ? d.NhanVien.HoTen : "Không xác định",
                    NgayBatDau = d.NgayBatDau,
                    NgayKetThuc = d.NgayKetThuc,
                    SoNgayNghi = d.SoNgayNghi,
                    NgayGuiDon = d.NgayGuiDon,
                    LyDo = d.LyDo,
                    TepDinhKem = d.TepDinhKem,
                    TrangThai = d.TrangThai,
                    RemainingLeaveDays = remaining // <--- TRƯỜNG MỚI
                };
            });

            return Ok(result);
        }

        [HttpGet("pending")]
        [Authorize(Roles = "Nhân sự phòng,Trưởng phòng,Nhân sự tổng,Giám đốc")]
        public async Task<ActionResult<IEnumerable<object>>> GetPendingRequests()
        {
            return await GetAllRequests(LeaveRequestStatus.Pending);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DonNghiPhep>> GetDonNghiPhepById(int id)
        {
            var donNghiPhep = await _context.DonNghiPheps.FindAsync(id);
            if (donNghiPhep == null) return NotFound();
            return donNghiPhep;
        }

        [HttpPost("approve/{id}")]
        [Authorize(Roles = "Nhân sự phòng,Trưởng phòng,Nhân sự tổng,Giám đốc")]
        public async Task<IActionResult> ApproveRequest(int id)
        {
            var request = await _context.DonNghiPheps.Include(d => d.NhanVien).FirstOrDefaultAsync(d => d.Id == id);

            if (request == null || request.TrangThai != LeaveRequestStatus.Pending)
            {
                return NotFound("Không tìm thấy đơn hoặc đơn đã được xử lý.");
            }

            var currentUserRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role")?.Value;
            var currentUserMaPhongBan = User.Claims.FirstOrDefault(c => c.Type == "MaPhongBan")?.Value;
            var roleClean = ConvertToUnSign(currentUserRole);

            if (roleClean == "truong phong" || roleClean == "nhan su phong")
            {
                if (request.NhanVien != null && request.NhanVien.MaPhongBan != currentUserMaPhongBan)
                {
                    return Forbid("Bạn không có quyền duyệt đơn của nhân viên phòng khác.");
                }
            }

            // ***** CẬP NHẬT LOGIC: LUÔN TÍNH CÔNG KHI DUYỆT (BỎ CHECK LÝ DO) *****
            // Logic: Cứ duyệt là chạy vòng lặp tính ngày
            for (var date = request.NgayBatDau; date.Date <= request.NgayKetThuc.Date; date = date.AddDays(1))
            {
                // Vẫn giữ bỏ qua T7, CN (theo quy định chung)
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                {
                    continue;
                }

                var requestYear = date.Year;
                var startDateOfYear = new DateTime(requestYear, 1, 1);

                // Đếm số ngày phép đã dùng trong năm
                var paidLeaveDaysTaken = await _context.ChamCongs
                    .CountAsync(c =>
                        c.MaNhanVien == request.MaNhanVien &&
                        c.NgayChamCong.Year == requestYear &&
                        c.NgayCong == 1.0 && !string.IsNullOrEmpty(c.GhiChu) &&
                        (c.GhiChu.Contains("Nghỉ phép") || c.GhiChu.Contains("Nghỉ có phép")));

                // Nếu còn phép (<12) -> Tính 1 công (Có lương)
                // Nếu hết phép (>=12) -> Tính 0 công (Không lương)
                double ngayCongValue = (paidLeaveDaysTaken < 12) ? 1.0 : 0.0;
                string ghiChuMoi = (ngayCongValue == 1.0) ? $"Nghỉ có phép: {request.LyDo}" : $"Nghỉ không phép (đã hết phép năm): {request.LyDo}";

                var existingChamCong = await _context.ChamCongs
                    .FirstOrDefaultAsync(c =>
                        c.MaNhanVien == request.MaNhanVien &&
                        c.NgayChamCong.Date == date.Date);

                if (existingChamCong != null)
                {
                    existingChamCong.NgayCong = ngayCongValue;
                    existingChamCong.GhiChu = ghiChuMoi;
                    existingChamCong.GioCheckOut = null;
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
            return Ok(new { message = "Duyệt đơn thành công." });
        }

        [HttpPost("reject/{id}")]
        [Authorize(Roles = "Nhân sự phòng,Trưởng phòng,Nhân sự tổng,Giám đốc")]
        public async Task<IActionResult> RejectRequest(int id)
        {
            var request = await _context.DonNghiPheps.Include(d => d.NhanVien).FirstOrDefaultAsync(d => d.Id == id);

            if (request == null || request.TrangThai != LeaveRequestStatus.Pending)
            {
                return NotFound("Không tìm thấy đơn hoặc đơn đã được xử lý.");
            }

            var currentUserRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role")?.Value;
            var currentUserMaPhongBan = User.Claims.FirstOrDefault(c => c.Type == "MaPhongBan")?.Value;
            var roleClean = ConvertToUnSign(currentUserRole);

            if (roleClean == "truong phong" || roleClean == "nhan su phong")
            {
                if (request.NhanVien != null && request.NhanVien.MaPhongBan != currentUserMaPhongBan)
                {
                    return Forbid("Bạn không có quyền từ chối đơn của nhân viên phòng khác.");
                }
            }

            request.TrangThai = LeaveRequestStatus.Rejected;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã từ chối đơn." });
        }
    }
}