using HRApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChuyenNganhController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ChuyenNganhController(AppDbContext context) { _context = context; }

        [HttpGet]
        public async Task<IActionResult> GetChuyenNganhs()
        {
            return Ok(await _context.ChuyenNganhs.ToListAsync());
        }
    }
}