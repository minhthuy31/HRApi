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
    public class ChamCongController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ChamCongController(AppDbContext context) { _context = context; }

        // GET: api/ChamCong?year=2025&month=8
        [HttpGet]
        public async Task<IActionResult> GetChamCongThang([FromQuery] int year, [FromQuery] int month)
        {
            if (year < 1 || year > 9999)
                return BadRequest($"Năm không hợp lệ: {year}. Phải nằm trong khoảng 1–9999.");

            if (month < 1 || month > 12)
                return BadRequest($"Tháng không hợp lệ: {month}. Phải nằm trong khoảng 1–12.");

            var startDateOfMonth = new DateTime(year, month, 1);
            var endDateOfMonth = startDateOfMonth.AddMonths(1);

            var chamCongDataForMonth = await _context.ChamCongs
                .Where(c => c.NgayChamCong >= startDateOfMonth && c.NgayChamCong < endDateOfMonth)
                .ToListAsync();

            var employeeIdsInMonth = chamCongDataForMonth.Select(c => c.MaNhanVien).Distinct().ToList();

            if (!employeeIdsInMonth.Any())
            {
                return Ok(new { DailyRecords = new List<object>(), Summaries = new Dictionary<string, object>() });
            }

            var startDateOfYear = new DateTime(year, 1, 1);
            var endDateOfYear = startDateOfYear.AddYears(1);

            var allPaidLeaveInYear = await _context.ChamCongs
                .Where(c => employeeIdsInMonth.Contains(c.MaNhanVien) &&
                              c.NgayChamCong >= startDateOfYear && c.NgayChamCong < endDateOfYear &&
                              c.NgayCong == 1.0 && !string.IsNullOrEmpty(c.GhiChu))
                .GroupBy(c => c.MaNhanVien)
                .Select(g => new { MaNhanVien = g.Key, DaysTaken = g.Count() })
                .ToDictionaryAsync(x => x.MaNhanVien, x => x.DaysTaken);

            var summaries = chamCongDataForMonth
                .GroupBy(c => c.MaNhanVien)
                .Select(g =>
                {
                    int annualLeaveAllowance = 12;
                    allPaidLeaveInYear.TryGetValue(g.Key, out int paidLeaveDaysTakenThisYear);
                    return new
                    {
                        MaNhanVien = g.Key,
                        DiLamDu = g.Count(c => c.NgayCong == 1.0 && string.IsNullOrEmpty(c.GhiChu)),
                        NghiCoPhep = g.Count(c => c.NgayCong == 1.0 && !string.IsNullOrEmpty(c.GhiChu)),
                        LamNuaNgay = g.Count(c => c.NgayCong == 0.5),
                        NghiKhongPhep = g.Count(c => c.NgayCong == 0.0),
                        TongCong = g.Sum(c => c.NgayCong),
                        RemainingLeaveDays = annualLeaveAllowance - paidLeaveDaysTakenThisYear
                    };
                }).ToDictionary(s => s.MaNhanVien);

            var dailyRecords = chamCongDataForMonth.Select(c => new
            {
                c.Id,
                c.MaNhanVien,
                NgayChamCong = c.NgayChamCong.Date.ToString("yyyy-MM-dd"),
                c.NgayCong,
                c.GhiChu
            }).ToList();

            return Ok(new { DailyRecords = dailyRecords, Summaries = summaries });

        }

        [HttpGet("{maNhanVien}")]
        public async Task<IActionResult> GetChamCongNhanVien(string maNhanVien, [FromQuery] int year, [FromQuery] int month)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (currentUserRole == "Nhân viên" && currentUserId != maNhanVien)
            {
                return Forbid("Bạn không có quyền xem bảng công của người khác.");
            }

            if (year < 1 || month < 1 || month > 12)
                return BadRequest("Năm hoặc tháng không hợp lệ.");

            var startDateOfMonth = new DateTime(year, month, 1);
            var endDateOfMonth = startDateOfMonth.AddMonths(1);

            // SỬA LỖI: Thêm điều kiện .Where(c => c.MaNhanVien == maNhanVien)
            // để chỉ lấy dữ liệu của đúng nhân viên đang xem.
            var chamCongDataForMonth = await _context.ChamCongs
                .Where(c => c.MaNhanVien == maNhanVien && c.NgayChamCong >= startDateOfMonth && c.NgayChamCong < endDateOfMonth)
                .ToListAsync();

            var pendingLeaveRequests = await _context.DonNghiPheps
                .Where(dnp => dnp.MaNhanVien == maNhanVien &&
                                dnp.TrangThai == "Chờ duyệt" && // Sử dụng chuỗi trạng thái tiếng Việt
                                dnp.NgayNghi >= startDateOfMonth && dnp.NgayNghi < endDateOfMonth)
                .ToListAsync();

            // Logic tính toán summary không cần lấy employeeIdsInMonth nữa vì đã lọc từ đầu
            var startDateOfYear = new DateTime(year, 1, 1);

            var paidLeaveDaysTakenThisYear = await _context.ChamCongs
                .CountAsync(c => c.MaNhanVien == maNhanVien &&
                              c.NgayChamCong >= startDateOfYear && c.NgayChamCong.Year == year &&
                              c.NgayCong == 1.0 && !string.IsNullOrEmpty(c.GhiChu));

            var summaryForEmployee = new
            {
                MaNhanVien = maNhanVien,
                DiLamDu = chamCongDataForMonth.Count(c => c.NgayCong == 1.0 && string.IsNullOrEmpty(c.GhiChu)),
                NghiCoPhep = chamCongDataForMonth.Count(c => c.NgayCong == 1.0 && !string.IsNullOrEmpty(c.GhiChu)),
                LamNuaNgay = chamCongDataForMonth.Count(c => c.NgayCong == -0.5),
                NghiKhongPhep = chamCongDataForMonth.Count(c => c.NgayCong == 0.0),
                TongCong = chamCongDataForMonth.Sum(c => c.NgayCong),
                RemainingLeaveDays = 12 - paidLeaveDaysTakenThisYear
            };

            var summaries = new Dictionary<string, object>
            {
                [maNhanVien] = summaryForEmployee
            };

            var dailyRecords = chamCongDataForMonth.Select(c => new
            {
                c.Id,
                c.MaNhanVien,
                NgayChamCong = c.NgayChamCong.Date.ToString("yyyy-MM-dd"),
                c.NgayCong,
                c.GhiChu
            }).ToList();

            return Ok(new
            {
                DailyRecords = dailyRecords,
                Summaries = summaries,
                PendingRequests = pendingLeaveRequests
            });
        }


        // POST: api/ChamCong/upsert
        [HttpPost("upsert")]
        public async Task<IActionResult> UpsertChamCong(ChamCong chamCongRequest)
        {

            bool wasConverted = false;

            if (chamCongRequest.NgayCong == 1.0 && !string.IsNullOrEmpty(chamCongRequest.GhiChu))
            {
                var requestYear = chamCongRequest.NgayChamCong.Year;
                var startDateOfYear = new DateTime(requestYear, 1, 1);
                var endDateOfYear = startDateOfYear.AddYears(1);

                var paidLeaveDaysTaken = await _context.ChamCongs
                    .CountAsync(c =>
                        c.MaNhanVien == chamCongRequest.MaNhanVien &&
                        c.NgayChamCong >= startDateOfYear && c.NgayChamCong < endDateOfYear &&
                        c.NgayCong == 1.0 && !string.IsNullOrEmpty(c.GhiChu) &&
                        c.Id != chamCongRequest.Id);

                if (paidLeaveDaysTaken >= 12)
                {
                    chamCongRequest.NgayCong = 0.0;
                    wasConverted = true;
                }
            }

            var existingRecord = await _context.ChamCongs
                .AsNoTracking()
                .FirstOrDefaultAsync(c =>
                    c.MaNhanVien == chamCongRequest.MaNhanVien &&
                    c.NgayChamCong.Date == chamCongRequest.NgayChamCong.Date);

            if (existingRecord != null)
            {
                chamCongRequest.Id = existingRecord.Id;
                _context.ChamCongs.Update(chamCongRequest);
            }
            else
            {
                _context.ChamCongs.Add(chamCongRequest);
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Lưu chấm công thành công!", wasConverted });
        }
    }
}
