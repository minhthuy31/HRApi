using HRApi.Data;
using HRApi.DTOs;
using HRApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRApi.Controllers
{
    // Lớp hằng số trạng thái (Giữ nguyên)
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

        public DonNghiPhepController(AppDbContext context)
        {
            _context = context;
        }

        // --- CẬP NHẬT ---
        // Sửa lại để trả về đối tượng vừa tạo, giúp frontend cập nhật UI tức thì
        [HttpPost]
        public async Task<IActionResult> CreateDonNghiPhep([FromBody] DonNghiPhepCreateDto dto)
        {
            var ngayNghiChuanHoa = dto.NgayNghi.Date;

            if (ngayNghiChuanHoa < DateTime.Today)
            {
                return BadRequest(new { message = "Không thể đăng ký nghỉ cho một ngày trong quá khứ." });
            }

            // Kiểm tra xem đã có đơn cho ngày này chưa
            var existingRequest = await _context.DonNghiPheps
                .AnyAsync(d => d.MaNhanVien == dto.MaNhanVien && d.NgayNghi.Date == ngayNghiChuanHoa && d.TrangThai != LeaveRequestStatus.Rejected);

            if (existingRequest)
            {
                return BadRequest(new { message = "Bạn đã có đơn xin nghỉ hoặc đã được chấm công cho ngày này." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var donNghiPhep = new DonNghiPhep
            {
                MaNhanVien = dto.MaNhanVien,
                NgayNghi = ngayNghiChuanHoa,
                LyDo = dto.LyDo,
                NgayGuiDon = DateTime.UtcNow, // Dùng UtcNow để chuẩn hóa
                TrangThai = LeaveRequestStatus.Pending
            };

            _context.DonNghiPheps.Add(donNghiPhep);
            await _context.SaveChangesAsync();

            // Trả về 201 Created cùng với object vừa tạo
            return CreatedAtAction(nameof(GetDonNghiPhepById), new { id = donNghiPhep.Id }, donNghiPhep);
        }

        // --- THÊM MỚI ---
        // Endpoint để lấy tất cả đơn nghỉ phép cho trang quản lý có bộ lọc
        [HttpGet]
        [Authorize(Roles = "Nhân sự phòng,Trưởng phòng,Nhân sự tổng,Giám đốc")]
        public async Task<ActionResult<IEnumerable<DonNghiPhepDto>>> GetAllRequests()
        {
            var requests = await _context.DonNghiPheps
                .Include(d => d.NhanVien)
                .Select(d => new DonNghiPhepDto
                {
                    Id = d.Id,
                    MaNhanVien = d.MaNhanVien,
                    HoTenNhanVien = d.NhanVien != null ? d.NhanVien.HoTen : "Không xác định",
                    NgayNghi = d.NgayNghi,
                    NgayGuiDon = d.NgayGuiDon,
                    LyDo = d.LyDo,
                    TrangThai = d.TrangThai
                })
                .OrderByDescending(d => d.NgayGuiDon)
                .ToListAsync();

            return Ok(requests);
        }

        // Endpoint cũ của bạn, có thể giữ lại hoặc không tùy vào nhu cầu
        [HttpGet("pending")]
        [Authorize(Roles = "Nhân sự phòng,Trưởng phòng,Nhân sự tổng,Giám đốc")]
        public async Task<ActionResult<IEnumerable<DonNghiPhepDto>>> GetPendingRequests()
        {
            // Logic cũ của bạn ở đây...
            // Tạm thời trả về endpoint mới để logic đồng nhất
            return await GetAllRequests();
        }

        // --- THÊM MỚI (Helper method) ---
        // Cần thiết để CreatedAtAction ở trên hoạt động
        [HttpGet("{id}")]
        public async Task<ActionResult<DonNghiPhep>> GetDonNghiPhepById(int id)
        {
            var donNghiPhep = await _context.DonNghiPheps.FindAsync(id);

            if (donNghiPhep == null)
            {
                return NotFound();
            }
            // Logic kiểm tra quyền xem chi tiết nếu cần
            return donNghiPhep;
        }


        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Nhân sự phòng,Trưởng phòng,Nhân sự tổng,Giám đốc")]
        public async Task<IActionResult> ApproveRequest(int id)
        {
            var request = await _context.DonNghiPheps.FindAsync(id);
            if (request == null || request.TrangThai != LeaveRequestStatus.Pending)
            {
                return NotFound("Không tìm thấy đơn hoặc đơn đã được xử lý.");
            }

            // 1. Logic kiểm tra số ngày phép còn lại trong năm (không đổi)
            const int annualLeaveAllowance = 12;
            var requestYear = request.NgayNghi.Year;
            var paidLeaveDaysTaken = await _context.ChamCongs
                .CountAsync(c =>
                    c.MaNhanVien == request.MaNhanVien &&
                    c.NgayChamCong.Year == requestYear &&
                    c.NgayCong == 1.0 && !string.IsNullOrEmpty(c.GhiChu));

            double ngayCongValue = 1.0;
            bool wasConverted = false;

            if (paidLeaveDaysTaken >= annualLeaveAllowance)
            {
                ngayCongValue = 0.0;
                wasConverted = true;
            }

            // SỬA LỖI Ở ĐÂY: Tạo ra chuỗi ghi chú mới kết hợp lý do gốc và trạng thái
            string ghiChuMoi = $"{request.LyDo} (đã duyệt)";

            // 2. Logic Upsert (Update/Insert) vào bảng Chấm Công
            var existingChamCong = await _context.ChamCongs
                .FirstOrDefaultAsync(c =>
                    c.MaNhanVien == request.MaNhanVien &&
                    c.NgayChamCong.Date == request.NgayNghi.Date);

            if (existingChamCong != null) // Nếu đã có bản ghi -> Cập nhật
            {
                existingChamCong.NgayCong = ngayCongValue;
                existingChamCong.GhiChu = ghiChuMoi; // Dùng ghi chú mới
                _context.ChamCongs.Update(existingChamCong);
            }
            else // Nếu chưa có -> Tạo mới
            {
                var newChamCong = new ChamCong
                {
                    MaNhanVien = request.MaNhanVien,
                    NgayChamCong = request.NgayNghi,
                    NgayCong = ngayCongValue,
                    GhiChu = ghiChuMoi // Dùng ghi chú mới
                };
                _context.ChamCongs.Add(newChamCong);
            }

            // 3. Cập nhật trạng thái đơn
            request.TrangThai = LeaveRequestStatus.Approved;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Duyệt đơn thành công.", wasConverted = wasConverted });
        }


        // Phương thức từ chối đơn (Giữ nguyên)
        [HttpPut("{id}/reject")]
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