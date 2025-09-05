using HRApi.Data;
using HRApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhongBanController : ControllerBase
    {
        private readonly AppDbContext _context;
        public PhongBanController(AppDbContext context) { _context = context; }

        // GET: api/PhongBan
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PhongBan>>> GetPhongBans([FromQuery] string? searchTerm)
        {
            var query = _context.PhongBans.AsQueryable();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(pb => pb.TenPhongBan.Contains(searchTerm)
                                       || pb.MaPhongBan.Contains(searchTerm));
            }
            return await query.OrderBy(pb => pb.TenPhongBan).ToListAsync();
        }

        // GET: api/PhongBan/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PhongBan>> GetPhongBan(string id)
        {
            var phongBan = await _context.PhongBans.FindAsync(id);
            if (phongBan == null) return NotFound("Không tìm thấy phòng ban.");
            return phongBan;
        }

        // POST: api/PhongBan
        [HttpPost]
        public async Task<ActionResult<PhongBan>> CreatePhongBan([FromBody] PhongBan phongBan)
        {
            if (string.IsNullOrWhiteSpace(phongBan.TenPhongBan))
            {
                return BadRequest("Tên phòng ban là bắt buộc.");
            }

            // Lấy toàn bộ mã phòng ban hiện tại
            var allMaPBs = await _context.PhongBans
                .Select(pb => pb.MaPhongBan)
                .ToListAsync();

            int maxId = 0;
            if (allMaPBs.Any())
            {
                maxId = allMaPBs
                    .Where(ma => ma != null && ma.Length > 2 && ma.StartsWith("PB"))
                    .Select(ma => int.TryParse(ma.Substring(2), out var id) ? id : 0)
                    .DefaultIfEmpty(0).Max();
            }

            string newMaPB = $"PB{(maxId + 1):D2}";
            phongBan.MaPhongBan = newMaPB;

            _context.PhongBans.Add(phongBan);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPhongBan), new { id = phongBan.MaPhongBan }, phongBan);
        }

        // PUT: api/PhongBan/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePhongBan(string id, [FromBody] PhongBan phongBan)
        {
            if (id != phongBan.MaPhongBan)
                return BadRequest("Mã phòng ban không khớp.");

            var existing = await _context.PhongBans.FindAsync(id);
            if (existing == null) return NotFound("Không tìm thấy phòng ban.");

            // Chỉ cập nhật field cho phép
            existing.TenPhongBan = phongBan.TenPhongBan;
            existing.DiaChi = phongBan.DiaChi;
            existing.sdt_PhongBan = phongBan.sdt_PhongBan;

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest($"Lỗi khi cập nhật phòng ban: {ex.Message}");
            }
        }

        // DELETE: api/PhongBan/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhongBan(string id)
        {
            var phongBan = await _context.PhongBans.FindAsync(id);
            if (phongBan == null) return NotFound("Không tìm thấy phòng ban.");

            try
            {
                _context.PhongBans.Remove(phongBan);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException)
            {
                return BadRequest("Không thể xóa phòng ban này vì có nhân viên đang liên kết.");
            }
        }
    }
}
