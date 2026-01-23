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

        // Helper function để check quyền Admin (Nhân sự trưởng + Giám đốc)
        private bool IsAdminOrHRManager()
        {
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            return role == "Nhân sự trưởng" || role == "Giám đốc";
        }

        #region Get Methods
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PhongBan>>> GetPhongBans([FromQuery] string? searchTerm, [FromQuery] bool? trangThai)
        {
            var currentUserRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var currentUserMaPhongBan = User.Claims.FirstOrDefault(c => c.Type == "MaPhongBan")?.Value;

            var query = _context.PhongBans.AsQueryable();

            // --- LOGIC PHÂN QUYỀN XEM DANH SÁCH ---
            if (currentUserRole == "Trưởng phòng")
            {
                // Trưởng phòng chỉ thấy phòng của mình
                if (!string.IsNullOrEmpty(currentUserMaPhongBan))
                {
                    query = query.Where(pb => pb.MaPhongBan == currentUserMaPhongBan);
                }
                else
                {
                    // Trưởng phòng mà không có mã phòng ban trong Token -> Không thấy gì
                    return Ok(new List<PhongBan>());
                }
            }
            else if (currentUserRole == "Kế toán trưởng" ||
                     currentUserRole == "Giám đốc" ||
                     currentUserRole == "Tổng giám đốc" ||
                     currentUserRole == "Nhân sự trưởng")
            {
                // Các role này được xem hết (Full list)
            }
            else
            {
                // Nhân viên thường không được xem danh sách phòng ban
                return Ok(new List<PhongBan>());
            }

            // Filter tìm kiếm
            if (trangThai.HasValue)
            {
                query = query.Where(pb => pb.TrangThai == trangThai.Value);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var lowerSearch = searchTerm.ToLower();
                query = query.Where(pb =>
                    (pb.TenPhongBan != null && pb.TenPhongBan.ToLower().Contains(lowerSearch)) ||
                    (pb.MaPhongBan != null && pb.MaPhongBan.ToLower().Contains(lowerSearch)));
            }

            return await query.OrderBy(pb => pb.TenPhongBan).ToListAsync();
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<PhongBan>> GetPhongBan(string id)
        {
            var currentUserRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var currentUserMaPhongBan = User.Claims.FirstOrDefault(c => c.Type == "MaPhongBan")?.Value;

            // --- LOGIC CHẶN XEM CHI TIẾT ---
            if (currentUserRole == "Trưởng phòng") // Sửa chính tả
            {
                // Nếu không có mã phòng ban hoặc mã phòng ban yêu cầu khác mã phòng ban của mình
                if (string.IsNullOrEmpty(currentUserMaPhongBan) || id != currentUserMaPhongBan)
                {
                    return Forbid("Bạn chỉ được xem thông tin phòng ban của mình.");
                }
            }
            else if (currentUserRole != "Kế toán trưởng" &&
                     currentUserRole != "Giám đốc" &&
                     currentUserRole != "Tổng giám đốc" &&
                     currentUserRole != "Nhân sự trưởng")
            {
                // Các role nhân viên thường bị chặn
                return Forbid("Bạn không có quyền xem chi tiết phòng ban.");
            }

            var phongBan = await _context.PhongBans.FirstOrDefaultAsync(pb => pb.MaPhongBan == id);

            if (phongBan == null) return NotFound("Không tìm thấy phòng ban.");
            return phongBan;
        }
        #endregion

        #region Create, Update, Delete (Chỉ Nhân sự trưởng & Giám đốc)

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<PhongBan>> CreatePhongBan([FromBody] PhongBan phongBan)
        {
            // --- CHECK QUYỀN: CHỈ NHÂN SỰ TRƯỞNG & GIÁM ĐỐC ---
            if (!IsAdminOrHRManager())
            {
                return StatusCode(403, "Chỉ Nhân sự trưởng và Giám đốc mới có quyền thêm phòng ban.");
            }

            if (string.IsNullOrWhiteSpace(phongBan.TenPhongBan))
            {
                return BadRequest("Tên phòng ban là bắt buộc.");
            }

            // Tự động sinh mã PB (PB01, PB02...)
            var allMaPBs = await _context.PhongBans.Select(pb => pb.MaPhongBan).ToListAsync();
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
            // --- CHECK QUYỀN: CHỈ NHÂN SỰ TRƯỞNG & GIÁM ĐỐC ---
            if (!IsAdminOrHRManager())
            {
                return StatusCode(403, "Chỉ Nhân sự trưởng và Giám đốc mới có quyền sửa đổi thông tin phòng ban.");
            }

            if (id != phongBan.MaPhongBan) return BadRequest("Mã phòng ban không khớp.");

            var existing = await _context.PhongBans.FindAsync(id);
            if (existing == null) return NotFound("Không tìm thấy phòng ban.");

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
        [HttpPost("{id}/disable")]
        public async Task<IActionResult> DisablePhongBan(string id)
        {
            // --- CHECK QUYỀN: CHỈ NHÂN SỰ TRƯỞNG & GIÁM ĐỐC ---
            if (!IsAdminOrHRManager())
            {
                return StatusCode(403, "Chỉ Nhân sự trưởng và Giám đốc mới có quyền vô hiệu hóa phòng ban.");
            }

            var phongBan = await _context.PhongBans.FindAsync(id);
            if (phongBan == null) return NotFound("Không tìm thấy phòng ban.");

            phongBan.TrangThai = false;
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Phòng ban '{phongBan.TenPhongBan}' đã được vô hiệu hóa." });
        }

        [Authorize]
        [HttpPost("{id}/activate")]
        public async Task<IActionResult> ActivatePhongBan(string id)
        {
            // --- CHECK QUYỀN: CHỈ NHÂN SỰ TRƯỞNG & GIÁM ĐỐC ---
            if (!IsAdminOrHRManager())
            {
                return StatusCode(403, "Chỉ Nhân sự trưởng và Giám đốc mới có quyền kích hoạt lại phòng ban.");
            }

            var phongBan = await _context.PhongBans.FindAsync(id);
            if (phongBan == null) return NotFound("Không tìm thấy phòng ban.");

            phongBan.TrangThai = true;
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Phòng ban '{phongBan.TenPhongBan}' đã được kích hoạt lại." });
        }
        #endregion
    }
}