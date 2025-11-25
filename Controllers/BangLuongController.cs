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
                        existingRecord.LuongThucNhan = record.LuongThucNhan;
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