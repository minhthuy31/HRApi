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
    public class ChamCongController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ChamCongController(AppDbContext context) { _context = context; }

        // ***** DTO CHO CHECK-IN QR *****
        public class CheckInQRDto
        {
            public string QrToken { get; set; }
        }

        // ***** HÀM CHECK-IN / CHECK-OUT BẰNG QR (ĐÃ CẬP NHẬT) *****
        [HttpPost("check-in-qr")]
        public async Task<IActionResult> CheckInWithQr([FromBody] CheckInQRDto dto)
        {
            // 1. Lấy mã nhân viên từ Token (biết ai đang check-in)
            var maNhanVien = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (maNhanVien == null)
            {
                return Unauthorized();
            }

            // 2. Tìm mã QR trong DB
            var qrToken = await _context.ActiveQRTokens
                .FirstOrDefaultAsync(t => t.Token == dto.QrToken);

            // 3. Kiểm tra mã
            if (qrToken == null)
            {
                return BadRequest(new { message = "Mã QR không hợp lệ." });
            }
            if (qrToken.IsUsed)
            {
                return BadRequest(new { message = "Mã QR đã được sử dụng." });
            }
            if (qrToken.ExpiresAt < DateTime.UtcNow)
            {
                return BadRequest(new { message = "Mã QR đã hết hạn. Vui lòng quét lại." });
            }

            // 4. MỌI THỨ HỢP LỆ -> Xử lý Check-in/Check-out
            var today = DateTime.Today;
            var existing = await _context.ChamCongs
                .FirstOrDefaultAsync(c => c.MaNhanVien == maNhanVien && c.NgayChamCong.Date == today);

            // Đánh dấu mã QR đã dùng ngay lập tức
            qrToken.IsUsed = true;

            if (existing != null)
            {
                // --- LOGIC CHECK-OUT ---
                if (existing.GioCheckOut != null)
                {
                    return BadRequest(new { message = $"Bạn đã check-out lúc {existing.GioCheckOut:HH:mm} hôm nay rồi." });
                }

                var gioCheckIn = existing.NgayChamCong; // Lấy giờ check-in đã lưu
                var gioCheckOut = DateTime.Now;        // Giờ check-out hiện tại

                existing.GioCheckOut = gioCheckOut;
                existing.GhiChu = $"Check-in: {gioCheckIn:HH:mm} | Check-out: {gioCheckOut:HH:mm}";

                // Tính toán tổng thời gian làm việc (logic đơn giản cho đồ án)
                // Giả sử: 8 tiếng = 1 công, 4 tiếng = 0.5 công
                var thoiGianLamViec = gioCheckOut - gioCheckIn;
                double totalHours = thoiGianLamViec.TotalHours;

                // Trừ thời gian nghỉ trưa (ví dụ: 1 tiếng) nếu làm cả ngày
                if (totalHours > 5) // Nếu làm trên 5 tiếng, giả sử có nghỉ trưa
                {
                    totalHours -= 1.0; // Trừ 1 tiếng nghỉ trưa
                }

                if (totalHours >= 7.5) // Làm 7.5 tiếng trở lên -> 1 công
                {
                    existing.NgayCong = 1.0;
                }
                else if (totalHours >= 3.5) // Làm từ 3.5 -> 7.5 tiếng -> 0.5 công
                {
                    existing.NgayCong = 0.5;
                }
                else // Làm dưới 3.5 tiếng -> 0 công
                {
                    existing.NgayCong = 0.0;
                }

                _context.ChamCongs.Update(existing);
                await _context.SaveChangesAsync();

                return Ok(new { message = $"Check-out thành công lúc {gioCheckOut:HH:mm}. Tổng giờ làm (đã trừ nghỉ trưa): {totalHours:F2} giờ." });
            }
            else
            {
                // --- LOGIC CHECK-IN ---
                var newChamCong = new ChamCong
                {
                    MaNhanVien = maNhanVien,
                    NgayChamCong = DateTime.Now, // Đây là Giờ Check-in
                    GioCheckOut = null,         // Chưa check-out
                    NgayCong = 0.0,             // Sẽ được tính khi check-out
                    GhiChu = "Check-in qua QR"
                };
                _context.ChamCongs.Add(newChamCong);
                await _context.SaveChangesAsync();

                return Ok(new { message = $"Check-in thành công lúc {newChamCong.NgayChamCong:HH:mm}!" });
            }
        }

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
                                dnp.NgayKetThuc >= startDateOfMonth &&
                              dnp.NgayBatDau < endDateOfMonth)
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
                LamNuaNgay = chamCongDataForMonth.Count(c => c.NgayCong == 0.5),
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
        public async Task<IActionResult> UpsertChamCong([FromBody] ChamCongUpsertDto dto)
        {
            // Kiểm tra xem chuỗi ngày tháng có hợp lệ không
            if (!DateTime.TryParse(dto.NgayChamCong, out DateTime ngayChamCongParsed))
            {
                return BadRequest(new { message = $"Định dạng ngày không hợp lệ: {dto.NgayChamCong}. Yêu cầu 'YYYY-MM-DD'." });
            }

            bool wasConverted = false;
            double finalNgayCong = dto.NgayCong;
            string finalGhiChu = dto.GhiChu;

            // Tìm bản ghi hiện có (tìm theo ngày, không phải giờ)
            var existingRecord = await _context.ChamCongs
                .FirstOrDefaultAsync(c =>
                    c.MaNhanVien == dto.MaNhanVien &&
                    c.NgayChamCong.Date == ngayChamCongParsed.Date); // So sánh .Date

            // Kiểm tra logic nghỉ phép
            if (dto.NgayCong == 1.0 && !string.IsNullOrEmpty(dto.GhiChu))
            {
                var requestYear = ngayChamCongParsed.Year;
                var startDateOfYear = new DateTime(requestYear, 1, 1);
                var endDateOfYear = startDateOfYear.AddYears(1);

                // Khi đếm, phải loại trừ chính bản ghi này ra (nếu nó đã tồn tại)
                int existingRecordId = existingRecord?.Id ?? 0;

                var paidLeaveDaysTaken = await _context.ChamCongs
                    .CountAsync(c =>
                        c.MaNhanVien == dto.MaNhanVien &&
                        c.NgayChamCong >= startDateOfYear && c.NgayChamCong < endDateOfYear &&
                        c.NgayCong == 1.0 && !string.IsNullOrEmpty(c.GhiChu) &&
                        c.Id != existingRecordId); // Loại trừ chính nó

                if (paidLeaveDaysTaken >= 12)
                {
                    finalNgayCong = 0.0; // Dùng biến tạm
                    finalGhiChu = "Hết phép, chuyển sang nghỉ không phép"; // Cập nhật ghi chú
                    wasConverted = true;
                }
            }

            if (existingRecord != null)
            {
                // CẬP NHẬT (Chỉ cập nhật 2 trường này, giữ nguyên GioCheckIn/Out)
                existingRecord.NgayCong = finalNgayCong;
                existingRecord.GhiChu = finalGhiChu;
                _context.ChamCongs.Update(existingRecord);
            }
            else
            {
                // TẠO MỚI
                var newRecord = new ChamCong
                {
                    MaNhanVien = dto.MaNhanVien,
                    NgayChamCong = ngayChamCongParsed, // Dùng ngày đã parse (lưu lúc 00:00)
                    GioCheckOut = null,
                    NgayCong = finalNgayCong,
                    GhiChu = finalGhiChu
                };
                _context.ChamCongs.Add(newRecord);
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Lưu chấm công thành công!", wasConverted });
        }
    }
}
