using HRApi.Data;
using HRApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BangLuongController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BangLuongController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/BangLuong?year=2025&month=11
        [HttpGet]
        public async Task<IActionResult> GetPayroll([FromQuery] int year, [FromQuery] int month)
        {
            var payrollData = await _context.BangLuongs
                .Where(b => b.Nam == year && b.Thang == month)
                .ToListAsync();
            return Ok(payrollData);
        }

        // GET: api/BangLuong/NV1?year=2025&month=12
        [HttpGet("{employeeId}")]
        public async Task<IActionResult> GetEmployeePayslip(string employeeId, [FromQuery] int year, [FromQuery] int month)
        {
            try
            {
                // 1. Lấy thông tin bảng lương tóm tắt (payslip)
                var payslip = await _context.BangLuongs
                    .FirstOrDefaultAsync(b => b.MaNhanVien == employeeId && b.Nam == year && b.Thang == month);

                if (payslip == null)
                {
                    // Quan trọng: Trả về 404 để frontend biết là "Không có dữ liệu"
                    return NotFound(new { message = "Không có dữ liệu lương cho tháng này." });
                }

                // 2. Lấy chi tiết chấm công (details)
                // Giả sử bạn có một DbSet tên là 'ChamCongs' trong AppDbContext
                // và model 'ChamCong' có các trường MaNhanVien, NgayChamCong, NgayCong, GhiChu
                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1);

                var attendanceDetails = await _context.ChamCongs // <== GIẢ SỬ BẠN CÓ DbSet NÀY
                    .Where(c => c.MaNhanVien == employeeId && c.NgayChamCong >= startDate && c.NgayChamCong < endDate)
                    .OrderBy(c => c.NgayChamCong)
                    .Select(c => new
                    {
                        Day = c.NgayChamCong.Day,
                        DayOfWeek = c.NgayChamCong.DayOfWeek.ToString(), // Frontend sẽ tự xử lý (vd: "Monday")
                        NgayCong = c.NgayCong,
                        GhiChu = c.GhiChu
                    })
                    .ToListAsync();

                // 3. Trả về đối tượng mà frontend (MyPayslipPage.js) mong đợi
                var payslipDetail = new
                {
                    Payslip = payslip,
                    Details = attendanceDetails
                };

                return Ok(payslipDetail);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server nội bộ: {ex.Message}");
            }
        }

        // POST: api/BangLuong/save
        [HttpPost("save")]
        public async Task<IActionResult> SavePayroll([FromBody] List<BangLuong> payrollData)
        {
            if (payrollData == null || !payrollData.Any())
            {
                return BadRequest("Không có dữ liệu bảng lương để lưu.");
            }

            var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var record in payrollData)
                {
                    var existingRecord = await _context.BangLuongs
                        .FirstOrDefaultAsync(b => b.MaNhanVien == record.MaNhanVien && b.Thang == record.Thang && b.Nam == record.Nam);

                    if (existingRecord != null)
                    {
                        // Cập nhật bản ghi đã tồn tại
                        existingRecord.LuongCoBan = record.LuongCoBan;
                        existingRecord.TongNgayCong = record.TongNgayCong;
                        //existingRecord.LuongThucNhan = record.LuongThucNhan;
                        existingRecord.NgayTinhLuong = DateTime.UtcNow;
                        _context.BangLuongs.Update(existingRecord);
                    }
                    else
                    {
                        // Thêm bản ghi mới
                        record.NgayTinhLuong = DateTime.UtcNow;
                        _context.BangLuongs.Add(record);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Lưu bảng lương thành công." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Lỗi server nội bộ: {ex.Message}");
            }
        }
    }
}