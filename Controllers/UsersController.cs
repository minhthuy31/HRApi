// Controllers/UsersController.cs

using HRApi.Data;
using HRApi.DTOs; // Thêm DTO
using HRApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
// using BCrypt.Net; // Thêm thư viện băm mật khẩu

namespace HRApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            // 1. Kiểm tra xem username hoặc email đã tồn tại chưa
            if (await _context.Users.AnyAsync(u => u.Username == createUserDto.Username))
            {
                return BadRequest(new { message = "Tên đăng nhập đã tồn tại." });
            }
            if (await _context.Users.AnyAsync(u => u.Email == createUserDto.Email))
            {
                return BadRequest(new { message = "Email đã được sử dụng." });
            }

            // 2. Tạo một đối tượng User mới
            var user = new User
            {
                Username = createUserDto.Username,
                Email = createUserDto.Email,
                // 3. BĂM MẬT KHẨU TRƯỚC KHI LƯU
                Password = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password),
                Role = "User",
                CreatedAt = DateTime.UtcNow,
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var userResponse = new { user.Id, user.Username, user.Email, user.Role };
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, userResponse);
        }


        // GET: api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // GET: api/users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return Ok(new { user.Id, user.Username, user.Email, user.Role }); // Không trả về password
        }

        // PUT: api/users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, User user)
        {
            if (id != user.Id) return BadRequest();
            // Cần có logic cập nhật an toàn hơn, nhưng tạm thời giữ nguyên
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}