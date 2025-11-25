using HRApi.Data;
using HRApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace HRApi.Controllers
{
    // Class hằng số trạng thái (Giữ nguyên)
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

            // Kiểm tra logic số ngày phép còn lại
            if (dto.LyDo.Contains("Nghỉ phép năm"))
            {
                var startDateOfYear = new DateTime(DateTime.Now.Year, 1, 1);
                // Đếm số ngày phép đã được duyệt (lấy từ bảng ChamCong)
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

            // 1. Xử lý file upload
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

            // 2. Tạo đối tượng DonNghiPhep
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
            var query = _context.DonNghiPheps.Include(d => d.NhanVien).AsQueryable();

            if (!string.IsNullOrEmpty(trangThai))
            {
                query = query.Where(d => d.TrangThai == trangThai);
            }

            var requests = await query
                .Select(d => new
                {
                    Id = d.Id,
                    MaNhanVien = d.MaNhanVien,
                    HoTenNhanVien = d.NhanVien != null ? d.NhanVien.HoTen : "Không xác định",
                    NgayBatDau = d.NgayBatDau,   // <-- SỬA LỖI
                    NgayKetThuc = d.NgayKetThuc, // <-- SỬA LỖI
                    SoNgayNghi = d.SoNgayNghi,   // <-- SỬA LỖI
                    NgayGuiDon = d.NgayGuiDon,
                    LyDo = d.LyDo,
                    TepDinhKem = d.TepDinhKem, // <-- THÊM MỚI
                    TrangThai = d.TrangThai
                })
                .OrderByDescending(d => d.NgayGuiDon)
                .ToListAsync();

            return Ok(requests);
        }

        [HttpGet("pending")]
        [Authorize(Roles = "Nhân sự phòng,Trưởng phòng,Nhân sự tổng,Giám đốc")]
        public async Task<ActionResult<IEnumerable<object>>> GetPendingRequests()
        {
            // Gọi hàm GetAllRequests với filter
            return await GetAllRequests(LeaveRequestStatus.Pending);
        }

        // HÀM NÀY GIỮ NGUYÊN
        [HttpGet("{id}")]
        public async Task<ActionResult<DonNghiPhep>> GetDonNghiPhepById(int id)
        {
            var donNghiPhep = await _context.DonNghiPheps.FindAsync(id);
            if (donNghiPhep == null)
            {
                return NotFound();
            }
            return donNghiPhep;
        }

        [HttpPost("approve/{id}")]
        [Authorize(Roles = "Nhân sự phòng,Trưởng phòng,Nhân sự tổng,Giám đốc")]
        public async Task<IActionResult> ApproveRequest(int id)
        {
            var request = await _context.DonNghiPheps.FindAsync(id);
            if (request == null || request.TrangThai != LeaveRequestStatus.Pending)
            {
                return NotFound("Không tìm thấy đơn hoặc đơn đã được xử lý.");
            }

            // Chỉ ghi vào bảng ChamCong nếu lý do là nghỉ được tính công (ví dụ)
            if (request.LyDo.Contains("Nghỉ phép") || request.LyDo.Contains("Nghỉ ốm"))
            {
                // Duyệt qua từng ngày trong đơn nghỉ
                for (var date = request.NgayBatDau; date.Date <= request.NgayKetThuc.Date; date = date.AddDays(1))
                {
                    // Bỏ qua Thứ 7 (DayOfWeek = 6) và Chủ Nhật (DayOfWeek = 0)
                    if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                    {
                        continue;
                    }

                    // Kiểm tra xem ngày này đã hết phép chưa
                    var requestYear = date.Year;
                    var startDateOfYear = new DateTime(requestYear, 1, 1);
                    var paidLeaveDaysTaken = await _context.ChamCongs
                        .CountAsync(c =>
                            c.MaNhanVien == request.MaNhanVien &&
                            c.NgayChamCong.Year == requestYear &&
                            c.NgayCong == 1.0 && !string.IsNullOrEmpty(c.GhiChu) &&
                            (c.GhiChu.Contains("Nghỉ phép") || c.GhiChu.Contains("Nghỉ có phép")));

                    // Logic tính công: 12 ngày phép
                    double ngayCongValue = (paidLeaveDaysTaken < 12) ? 1.0 : 0.0;
                    string ghiChuMoi = (ngayCongValue == 1.0) ? request.LyDo : "Nghỉ không phép (đã hết 12 ngày phép năm)";

                    // Tìm bản ghi chấm công
                    var existingChamCong = await _context.ChamCongs
                        .FirstOrDefaultAsync(c =>
                            c.MaNhanVien == request.MaNhanVien &&
                            c.NgayChamCong.Date == date.Date);

                    if (existingChamCong != null)
                    {
                        // Nếu nhân viên lỡ check-in/out, ghi đè
                        existingChamCong.NgayCong = ngayCongValue;
                        existingChamCong.GhiChu = ghiChuMoi;
                        existingChamCong.GioCheckOut = null; // Xóa giờ check-in/out
                    }
                    else
                    {
                        // Tạo mới bản ghi chấm công cho ngày này
                        _context.ChamCongs.Add(new ChamCong
                        {
                            MaNhanVien = request.MaNhanVien,
                            NgayChamCong = date.Date, // Giờ check-in là 00:00
                            NgayCong = ngayCongValue,
                            GhiChu = ghiChuMoi
                        });
                    }
                }
            }
            // Nếu lý do là "Nghỉ việc gia đình", "Khác"... (không phải nghỉ phép), 
            // thì không tự động tạo bản ghi chấm công (HR sẽ phải tự nhập 0.0)

            request.TrangThai = LeaveRequestStatus.Approved;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Duyệt đơn thành công." });
        }

        [HttpPost("reject/{id}")]
        [Authorize(Roles = "Nhân sự phòng,Trưởng phòng,Nhân sự tổng,Giám đốc")]
        public async Task<IActionResult> RejectRequest(int id)
        {
            var request = await _context.DonNghiPheps.FindAsync(id);
            if (request == null || request.TrangThai != LeaveRequestStatus.Pending)
            {
                return NotFound("Không tìm thấy đơn hoặc đơn đã được xử lý.");
            }

            request.TrangThai = LeaveRequestStatus.Rejected;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã từ chối đơn." });
        }
    }
}