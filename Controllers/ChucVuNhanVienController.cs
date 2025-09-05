using HRApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChucVuNhanVienController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ChucVuNhanVienController(AppDbContext context) { _context = context; }

        [HttpGet]
        public async Task<IActionResult> GetChucVus()
        {
            return Ok(await _context.ChucVuNhanViens.ToListAsync());
        }
    }
}