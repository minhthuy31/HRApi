using HRApi.Data;
using HRApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HRApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhongBanController : ControllerBase
    {
        private readonly AppDbContext _context;
        public PhongBanController(AppDbContext context) { _context = context; }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PhongBan>>> GetPhongBans([FromQuery] string? searchTerm, [FromQuery] bool? trangThai)
        {
            var currentUserRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var currentUserMaPhongBan = User.Claims.FirstOrDefault(c => c.Type == "MaPhongBan")?.Value;

            var query = _context.PhongBans.AsQueryable();
            if (currentUserRole == "Trưởng phòng" || currentUserRole == "Nhân sự phòng" || currentUserRole == "Nhân viên")
            {
                if (!string.IsNullOrEmpty(currentUserMaPhongBan))
                {
                    // Trưởng/Nhân sự phòng chỉ thấy phòng ban của chính họ
                    query = query.Where(pb => pb.MaPhongBan == currentUserMaPhongBan);
                }
                else
                {
                    return Ok(new List<PhongBan>());
                }
            }
            if (trangThai.HasValue)
            {
                query = query.Where(pb => pb.TrangThai == trangThai.Value);
            }
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(pb => pb.TenPhongBan.Contains(searchTerm) || pb.MaPhongBan.Contains(searchTerm));
            }
            return await query.OrderBy(pb => pb.TenPhongBan).ToListAsync();
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<PhongBan>> GetPhongBan(string id)
        {
            var phongBan = await _context.PhongBans.FindAsync(id);
            if (phongBan == null) return NotFound("Không tìm thấy phòng ban.");
            return phongBan;
        }

        [Authorize]
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

        [Authorize]
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
            existing.TrangThai = phongBan.TrangThai;

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

        [Authorize]
        // POST: api/PhongBan/PB01/disable
        [HttpPost("{id}/disable")]
        public async Task<IActionResult> DisablePhongBan(string id)
        {
            var phongBan = await _context.PhongBans.FindAsync(id);
            if (phongBan == null) return NotFound("Không tìm thấy phòng ban.");

            phongBan.TrangThai = false;
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Phòng ban '{phongBan.TenPhongBan}' đã được vô hiệu hóa." });
        }

        // POST: api/PhongBan/PB01/activate
        [HttpPost("{id}/activate")]
        public async Task<IActionResult> ActivatePhongBan(string id)
        {
            var phongBan = await _context.PhongBans.FindAsync(id);
            if (phongBan == null) return NotFound("Không tìm thấy phòng ban.");

            phongBan.TrangThai = true;
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Phòng ban '{phongBan.TenPhongBan}' đã được kích hoạt lại." });
        }
    }
}
