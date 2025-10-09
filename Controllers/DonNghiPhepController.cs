using HRApi.Data;
using HRApi.DTOs;
using HRApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        public DonNghiPhepController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateDonNghiPhep([FromBody] DonNghiPhepCreateDto dto)
        {
            var ngayNghiChuanHoa = dto.NgayNghi.Date;

            if (ngayNghiChuanHoa < DateTime.Today)
            {
                return BadRequest(new { message = "Không thể đăng ký nghỉ cho một ngày trong quá khứ." });
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
                NgayGuiDon = DateTime.Now,
                TrangThai = LeaveRequestStatus.Pending
            };

            _context.DonNghiPheps.Add(donNghiPhep);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Gửi đơn thành công" });
        }


        [HttpGet("pending")]
        [Authorize(Roles = "Nhân sự phòng,Trưởng phòng,Nhân sự tổng,Giám đốc")]
        public async Task<ActionResult<IEnumerable<DonNghiPhepDto>>> GetPendingRequests()
        {
            var requests = await _context.DonNghiPheps
                .Where(d => d.TrangThai == LeaveRequestStatus.Pending)
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

        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Nhân sự phòng,Trưởng phòng,Nhân sự tổng,Giám đốc")]
        public async Task<IActionResult> ApproveRequest(int id)
        {
            var request = await _context.DonNghiPheps.FindAsync(id);
            if (request == null || request.TrangThai != LeaveRequestStatus.Pending)
            {
                return NotFound("Không tìm thấy đơn hoặc đơn đã được xử lý.");
            }

            request.TrangThai = LeaveRequestStatus.Approved;

            var newChamCong = new ChamCong
            {
                MaNhanVien = request.MaNhanVien,
                NgayChamCong = request.NgayNghi,
                NgayCong = 0.5,
                GhiChu = $"Nghỉ phép (đơn #{id} đã duyệt)"
            };
            _context.ChamCongs.Add(newChamCong);

            await _context.SaveChangesAsync();
            return Ok(new { message = "Duyệt đơn thành công." });
        }

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

