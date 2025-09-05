using HRApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrinhDoHocVanController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TrinhDoHocVanController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetTrinhDoHocVans()
        {
            return Ok(await _context.TrinhDoHocVans.ToListAsync());
        }
    }
}
