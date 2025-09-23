using HRApi.Data;
using HRApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRApi.Controllers
{
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

            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var chamCongData = await _context.ChamCongs
                .Where(c => c.NgayChamCong >= startDate && c.NgayChamCong <= endDate)
                .Select(c => new
                {
                    c.Id,
                    c.MaNhanVien,
                    NgayChamCong = c.NgayChamCong.Date.ToString("yyyy-MM-dd"),
                    c.NgayCong,
                    c.GhiChu
                })
                .ToListAsync();

            return Ok(chamCongData);
        }

        // POST: api/ChamCong/upsert
        [HttpPost("upsert")]
        public async Task<IActionResult> UpsertChamCong(ChamCong chamCongRequest)
        {
            var existingRecord = await _context.ChamCongs
                .FirstOrDefaultAsync(c =>
                    c.MaNhanVien == chamCongRequest.MaNhanVien &&
                    c.NgayChamCong.Date == chamCongRequest.NgayChamCong.Date);

            if (existingRecord != null)
            {
                existingRecord.NgayCong = chamCongRequest.NgayCong;
                existingRecord.GhiChu = chamCongRequest.NgayCong == 0.5 ? chamCongRequest.GhiChu : null;//ghi chú nếu là 0.5 ngày(nghỉ có phép)
            }
            else
            {
                if (chamCongRequest.NgayCong != 0.5)
                {
                    chamCongRequest.GhiChu = null;
                }
                _context.ChamCongs.Add(chamCongRequest);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Lưu chấm công thành công!" });
        }
    }
}
