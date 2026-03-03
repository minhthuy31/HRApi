using HRApi.Data;
using HRApi.DTOs;
using HRApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace HRApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ChamCongController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ChamCongController(AppDbContext context) { _context = context; }

        public class CheckInQRDto
        {
            public string QrToken { get; set; }
        }

        public class LockDto
        {
            public int Year { get; set; }
            public int Month { get; set; }
        }

        // Helper: Check quyền Admin (HR hoặc Giám đốc)
        private bool IsAdminOrHR()
        {
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            return role == "Giám đốc" || role == "Nhân sự trưởng" || role == "Tổng giám đốc";
        }

        // --- 1. Check-in QR (Giữ nguyên) ---
        [HttpPost("check-in-qr")]
        public async Task<IActionResult> CheckInWithQr([FromBody] CheckInQRDto dto)
        {
            var maNhanVien = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (maNhanVien == null) return Unauthorized();

            var qrToken = await _context.ActiveQRTokens.FirstOrDefaultAsync(t => t.Token == dto.QrToken);
            if (qrToken == null) return BadRequest(new { message = "Mã QR không hợp lệ." });
            if (qrToken.IsUsed) return BadRequest(new { message = "Mã QR đã được sử dụng." });
            if (qrToken.ExpiresAt < DateTime.UtcNow) return BadRequest(new { message = "Mã QR đã hết hạn." });

            var today = DateTime.Today;
            var existing = await _context.ChamCongs.FirstOrDefaultAsync(c => c.MaNhanVien == maNhanVien && c.NgayChamCong.Date == today);

            qrToken.IsUsed = true;

            if (existing != null)
            {
                if (existing.GioCheckOut != null) return BadRequest(new { message = "Bạn đã check-out rồi." });

                existing.GioCheckOut = DateTime.Now;
                existing.GhiChu = $"Check-in: {existing.NgayChamCong:HH:mm} | Check-out: {existing.GioCheckOut:HH:mm}";

                double totalHours = (existing.GioCheckOut.Value - existing.NgayChamCong).TotalHours;
                if (totalHours > 5) totalHours -= 1.0;

                if (totalHours >= 7.5) existing.NgayCong = 1.0;
                else if (totalHours >= 3.5) existing.NgayCong = 0.5;
                else existing.NgayCong = 0.0;

                _context.ChamCongs.Update(existing);
                await _context.SaveChangesAsync();
                return Ok(new { message = $"Check-out thành công. Công: {existing.NgayCong}" });
            }
            else
            {
                var newChamCong = new ChamCong
                {
                    MaNhanVien = maNhanVien,
                    NgayChamCong = DateTime.Now,
                    NgayCong = 0.0,
                    GhiChu = "Check-in qua QR"
                };
                _context.ChamCongs.Add(newChamCong);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Check-in thành công!" });
            }
        }

        // --- 2. Lấy dữ liệu chấm công tháng (LOGIC PHÂN QUYỀN MỚI) ---
        [HttpGet]
        public async Task<IActionResult> GetChamCongThang([FromQuery] int year, [FromQuery] int month)
        {
            var currentUserRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var currentUserMaPhongBan = User.Claims.FirstOrDefault(c => c.Type == "MaPhongBan")?.Value;

            if (year < 1 || month < 1 || month > 12) return BadRequest("Thời gian sai.");

            // Check trạng thái khóa
            var lockRecord = await _context.KhoaCongs.FirstOrDefaultAsync(k => k.Nam == year && k.Thang == month);
            bool isLocked = lockRecord != null && lockRecord.IsLocked;

            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);

            // Include NhanVien để check phòng ban
            var query = _context.ChamCongs
                .Include(c => c.NhanVien)
                .Where(c => c.NgayChamCong >= startDate && c.NgayChamCong < endDate)
                .AsQueryable();

            // --- PHÂN QUYỀN ---
            if (currentUserRole == "Trưởng phòng")
            {
                if (!string.IsNullOrEmpty(currentUserMaPhongBan))
                {
                    // Trưởng phòng: Chỉ lấy nhân viên cùng phòng
                    query = query.Where(c => c.NhanVien.MaPhongBan == currentUserMaPhongBan);
                }
                else
                {
                    return Ok(new { DailyRecords = new List<object>(), Summaries = new Dictionary<string, object>(), IsLocked = isLocked });
                }
            }
            else if (IsAdminOrHR() || currentUserRole == "Kế toán trưởng")
            {
                // HR, Giám đốc, Kế toán: Xem hết
            }
            else
            {
                return Forbid("Bạn không có quyền xem bảng công tổng hợp.");
            }

            var data = await query.ToListAsync();

            // --- TÍNH SUMMARY ---
            var employeeIds = data.Select(c => c.MaNhanVien).Distinct().ToList();
            var startYear = new DateTime(year, 1, 1);
            var endYear = startYear.AddYears(1);

            var paidLeaves = await _context.ChamCongs
                .Where(c => employeeIds.Contains(c.MaNhanVien) &&
                       c.NgayChamCong >= startYear && c.NgayChamCong < endYear &&
                       c.NgayCong == 1.0 && !string.IsNullOrEmpty(c.GhiChu))
                .GroupBy(c => c.MaNhanVien)
                .ToDictionaryAsync(g => g.Key, g => g.Count());

            var summaries = data.GroupBy(c => c.MaNhanVien).ToDictionary(g => g.Key, g => new
            {
                MaNhanVien = g.Key,
                TongCong = g.Sum(c => c.NgayCong),
                DiLamDu = g.Count(c => c.NgayCong == 1.0 && string.IsNullOrEmpty(c.GhiChu)),
                NghiCoPhep = g.Count(c => c.NgayCong == 1.0 && !string.IsNullOrEmpty(c.GhiChu)),
                RemainingLeaveDays = 12 - (paidLeaves.ContainsKey(g.Key) ? paidLeaves[g.Key] : 0)
            });

            var dailyRecords = data.Select(c => new
            {
                c.Id,
                c.MaNhanVien,
                NgayChamCong = c.NgayChamCong.ToString("yyyy-MM-dd"),
                c.NgayCong,
                c.GhiChu,
                GioCheckIn = c.GioCheckIn,    
                GioCheckOut = c.GioCheckOut
            }).ToList();

            // Trả về IsLocked để FE biết đường disable nút sửa
            return Ok(new { DailyRecords = dailyRecords, Summaries = summaries, IsLocked = isLocked });
        }

        // --- 3. Lấy dữ liệu cá nhân ---
        [HttpGet("{maNhanVien}")]
        public async Task<IActionResult> GetChamCongNhanVien(string maNhanVien, [FromQuery] int year, [FromQuery] int month)
        {
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUserMaPhongBan = User.FindFirst("MaPhongBan")?.Value;

            // 1. Nhân viên thường chỉ xem của mình
            if (currentUserRole == "Nhân viên" && currentUserId != maNhanVien)
                return Forbid();

            // 2. Trưởng phòng chỉ xem nhân viên phòng mình
            if (currentUserRole == "Trưởng phòng")
            {
                var targetEmp = await _context.NhanViens.AsNoTracking().FirstOrDefaultAsync(nv => nv.MaNhanVien == maNhanVien);
                if (targetEmp == null || targetEmp.MaPhongBan != currentUserMaPhongBan)
                    return Forbid("Chỉ được xem nhân viên phòng mình.");
            }

            if (year < 1 || month < 1 || month > 12) return BadRequest("Lỗi thời gian.");
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);

            var data = await _context.ChamCongs
                .Where(c => c.MaNhanVien == maNhanVien && c.NgayChamCong >= startDate && c.NgayChamCong < endDate)
                .ToListAsync();

            var dailyRecords = data.Select(c => new
            {
                c.Id,
                c.MaNhanVien,
                NgayChamCong = c.NgayChamCong.ToString("yyyy-MM-dd"),
                c.NgayCong,
                c.GhiChu,
                GioCheckIn = c.GioCheckIn,    
                GioCheckOut = c.GioCheckOut   
            }).ToList();
            return Ok(new { DailyRecords = dailyRecords });
        }

        // --- 4. NHẬP / SỬA CÔNG (Upsert - CẬP NHẬT PHÂN QUYỀN & KHÓA CÔNG) ---
        [HttpPost("upsert")]
        public async Task<IActionResult> UpsertChamCong([FromBody] ChamCongUpsertDto dto)
        {
            var currentUserRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var currentUserMaPhongBan = User.Claims.FirstOrDefault(c => c.Type == "MaPhongBan")?.Value;

            // 1. CHẶN QUYỀN CƠ BẢN
            if (currentUserRole == "Kế toán trưởng" || currentUserRole == "Nhân viên")
            {
                return StatusCode(403, "Bạn chỉ có quyền xem, không được sửa đổi chấm công.");
            }

            if (!DateTime.TryParse(dto.NgayChamCong, out DateTime dateParsed)) return BadRequest("Ngày sai.");

            // 2. CHECK KHÓA CÔNG
            var isLocked = await _context.KhoaCongs.AnyAsync(k => k.Nam == dateParsed.Year && k.Thang == dateParsed.Month && k.IsLocked);
            if (isLocked)
            {
                return BadRequest(new { message = $"Bảng công tháng {dateParsed.Month}/{dateParsed.Year} đã bị khóa." });
            }

            // 3. CHECK TRƯỞNG PHÒNG
            var targetEmp = await _context.NhanViens.AsNoTracking().FirstOrDefaultAsync(nv => nv.MaNhanVien == dto.MaNhanVien);
            if (targetEmp == null) return BadRequest("NV không tồn tại.");

            if (currentUserRole == "Trưởng phòng")
            {
                if (string.IsNullOrEmpty(currentUserMaPhongBan) || targetEmp.MaPhongBan != currentUserMaPhongBan)
                {
                    return StatusCode(403, "Trưởng phòng chỉ được nhập công cho nhân viên thuộc phòng mình.");
                }
            }

            // 4. LOGIC LƯU DỮ LIỆU
            var existingRecord = await _context.ChamCongs.FirstOrDefaultAsync(c => c.MaNhanVien == dto.MaNhanVien && c.NgayChamCong.Date == dateParsed.Date);
            if (dto.OnlyIfEmpty && existingRecord != null)
            {
                // Nếu yêu cầu "Chỉ điền ô trống" MÀ ô này đã có dữ liệu -> Bỏ qua, không làm gì cả
                // Trả về OK để frontend không báo lỗi, coi như đã xử lý xong (skip)
                return Ok(new { message = "Skipped (Data exists)", skipped = true });
            }
            bool wasConverted = false;
            double finalCong = dto.NgayCong;
            string? finalGhiChu = dto.GhiChu;

            if (dto.NgayCong == 1.0 && !string.IsNullOrEmpty(dto.GhiChu))
            {
                var startY = new DateTime(dateParsed.Year, 1, 1);
                var endY = startY.AddYears(1);

                // Fix lỗi LINQ null propagate: tính ID trước
                int excludeId = existingRecord != null ? existingRecord.Id : 0;

                var taken = await _context.ChamCongs.CountAsync(c =>
                    c.MaNhanVien == dto.MaNhanVien && c.NgayChamCong >= startY && c.NgayChamCong < endY &&
                    c.NgayCong == 1.0 && !string.IsNullOrEmpty(c.GhiChu) && c.Id != excludeId);

                if (taken >= 12) { finalCong = 0.0; finalGhiChu = "Hết phép -> Không phép"; wasConverted = true; }
            }

            if (existingRecord != null)
            {
                existingRecord.NgayCong = finalCong;
                existingRecord.GhiChu = finalGhiChu;
                _context.ChamCongs.Update(existingRecord);
            }
            else
            {
                _context.ChamCongs.Add(new ChamCong
                {
                    MaNhanVien = dto.MaNhanVien,
                    NgayChamCong = dateParsed,
                    NgayCong = finalCong,
                    GhiChu = finalGhiChu,
                    GioCheckOut = null
                });
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Lưu thành công", wasConverted });
        }

        // --- 5. KHÓA CÔNG (Dành cho HR/Admin) ---
        [HttpPost("lock")]
        public async Task<IActionResult> LockChamCong([FromBody] LockDto dto)
        {
            if (!IsAdminOrHR()) return StatusCode(403, "Chỉ HR hoặc Giám đốc mới được khóa công.");

            var record = await _context.KhoaCongs.FirstOrDefaultAsync(k => k.Nam == dto.Year && k.Thang == dto.Month);
            if (record == null)
            {
                _context.KhoaCongs.Add(new KhoaCong { Nam = dto.Year, Thang = dto.Month, IsLocked = true });
            }
            else
            {
                record.IsLocked = true;
                _context.KhoaCongs.Update(record);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Đã khóa bảng công tháng {dto.Month}/{dto.Year}." });
        }
        // --- 5. KHÓA / HỦY KHÓA CÔNG ---
        public class LockActionDto
        {
            public int Year { get; set; }
            public int Month { get; set; }
            public bool IsLocked { get; set; } // true: Khóa, false: Hủy khóa
        }

        [HttpPost("lock-action")] // Đổi tên endpoint cho rõ nghĩa hơn hoặc dùng lại "lock"
        public async Task<IActionResult> LockOrUnlockChamCong([FromBody] LockActionDto dto)
        {
            if (!IsAdminOrHR()) return StatusCode(403, "Chỉ HR hoặc Giám đốc mới được thực hiện thao tác này.");

            var record = await _context.KhoaCongs.FirstOrDefaultAsync(k => k.Nam == dto.Year && k.Thang == dto.Month);

            if (record == null)
            {
                _context.KhoaCongs.Add(new KhoaCong { Nam = dto.Year, Thang = dto.Month, IsLocked = dto.IsLocked });
            }
            else
            {
                record.IsLocked = dto.IsLocked;
                _context.KhoaCongs.Update(record);
            }

            await _context.SaveChangesAsync();

            string actionText = dto.IsLocked ? "khóa" : "hủy khóa";
            return Ok(new { message = $"Đã {actionText} bảng công tháng {dto.Month}/{dto.Year}." });
        }

        // --- MỚI: API Chấm công bằng khuôn mặt ---
        // --- BỔ SUNG: API ĐĂNG KÝ KHUÔN MẶT (Fix lỗi 405) ---
        [HttpPost("register-face")]
        public async Task<IActionResult> RegisterFace([FromBody] RegisterFaceDto dto)
        {
            // 1. Kiểm tra dữ liệu đầu vào
            if (dto.FaceDescriptor == null || dto.FaceDescriptor.Length != 128)
            {
                return BadRequest(new { success = false, message = "Dữ liệu khuôn mặt không hợp lệ (Phải đủ 128 chiều)." });
            }

            // 2. Kiểm tra nhân viên có tồn tại không
            var nhanVien = await _context.NhanViens.FindAsync(dto.MaNhanVien);
            if (nhanVien == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy nhân viên này." });
            }

            // 3. Chuyển mảng float[] thành chuỗi JSON để lưu vào Database
            // Vì Model FaceData của bạn lưu FaceDescriptor dưới dạng string
            string jsonVector = JsonSerializer.Serialize(dto.FaceDescriptor);

            // 4. Kiểm tra xem nhân viên này đã có dữ liệu khuôn mặt chưa
            var existingFace = await _context.FaceDatas.FirstOrDefaultAsync(f => f.MaNhanVien == dto.MaNhanVien);

            if (existingFace != null)
            {
                // Nếu có rồi -> Cập nhật lại khuôn mặt mới
                existingFace.FaceDescriptor = jsonVector;
                _context.FaceDatas.Update(existingFace);
            }
            else
            {
                // Nếu chưa có -> Tạo mới
                var newFaceData = new FaceData
                {
                    MaNhanVien = dto.MaNhanVien,
                    FaceDescriptor = jsonVector
                };
                _context.FaceDatas.Add(newFaceData);
            }

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Đăng ký khuôn mặt thành công!" });
        }
        [HttpPost("check-in-face")]
        public async Task<IActionResult> CheckInWithFace([FromBody] CheckInFaceDto dto)
        {
            // 1. Lấy tất cả dữ liệu khuôn mặt trong DB ra để so sánh
            // Lưu ý: Với đồ án hoặc công ty nhỏ < 1000 NV thì cách này OK. 
            // Nếu dữ liệu lớn cần dùng Vector Database chuyên dụng.
            var allFaces = await _context.FaceDatas.ToListAsync();

            string foundEmployeeId = null;
            double bestMatchDistance = double.MaxValue; // Khoảng cách nhỏ nhất tìm thấy

            // 2. Thuật toán so sánh (Duyệt qua từng người trong DB)
            foreach (var face in allFaces)
            {
                // Giải nén chuỗi JSON trong DB ra thành mảng số
                var storedVector = JsonSerializer.Deserialize<float[]>(face.FaceDescriptor);

                // Tính khoảng cách giữa khuôn mặt gửi lên và khuôn mặt trong DB
                var distance = CalculateEuclideanDistance(dto.FaceDescriptor, storedVector);

                // Ngưỡng (Threshold) cho face-api.js thường là 0.45 - 0.5
                // Nếu khoảng cách nhỏ hơn 0.5 nghĩa là khớp
                if (distance < 0.5 && distance < bestMatchDistance)
                {
                    bestMatchDistance = distance;
                    foundEmployeeId = face.MaNhanVien;
                }
            }

            if (string.IsNullOrEmpty(foundEmployeeId))
            {
                return BadRequest(new { success = false, message = "Không nhận diện được khuôn mặt nhân viên." });
            }

            // 3. Nếu tìm thấy người -> Thực hiện logic chấm công (tương tự như QR Code)
            var today = DateTime.Today;
            var existing = await _context.ChamCongs.FirstOrDefaultAsync(c => c.MaNhanVien == foundEmployeeId && c.NgayChamCong.Date == today);

            if (existing != null)
            {
                // LOGIC CHECK-OUT (Ra về)
                if (existing.GioCheckOut != null)
                    return BadRequest(new { success = false, message = "Bạn đã check-out hôm nay rồi." });

                existing.GioCheckOut = DateTime.Now;
                existing.GhiChu = (existing.GhiChu ?? "") + $" | Face Check-out: {existing.GioCheckOut:HH:mm}";

                // Tính toán công dựa trên giờ làm
                double totalHours = (existing.GioCheckOut.Value - (existing.GioCheckIn ?? existing.NgayChamCong)).TotalHours;
                if (totalHours > 5) totalHours -= 1.0; // Trừ giờ nghỉ trưa

                if (totalHours >= 7.5) existing.NgayCong = 1.0;
                else if (totalHours >= 3.5) existing.NgayCong = 0.5;
                else existing.NgayCong = 0.0;

                _context.ChamCongs.Update(existing);
                await _context.SaveChangesAsync();

                var nv = await _context.NhanViens.FindAsync(foundEmployeeId);
                return Ok(new { success = true, message = $"Check-out thành công cho {nv?.HoTen}.", ngayCong = existing.NgayCong });
            }
            else
            {
                // LOGIC CHECK-IN (Vào làm)
                var newChamCong = new ChamCong
                {
                    MaNhanVien = foundEmployeeId,
                    NgayChamCong = DateTime.Now, // Ngày chấm công
                    GioCheckIn = DateTime.Now,   // Giờ vào thực tế
                    NgayCong = 0.0,
                    GhiChu = "Face Check-in",
                    LoaiNgayCong = "Làm việc"
                };

                // Logic đi muộn (Ví dụ sau 8:15 là muộn)
                if (DateTime.Now.Hour > 8 || (DateTime.Now.Hour == 8 && DateTime.Now.Minute > 15))
                {
                    newChamCong.DiMuon = true;
                    newChamCong.GhiChu += " (Đi muộn)";
                }

                _context.ChamCongs.Add(newChamCong);
                await _context.SaveChangesAsync();

                var nv = await _context.NhanViens.FindAsync(foundEmployeeId);
                return Ok(new { success = true, message = $"Check-in thành công cho {nv?.HoTen}!", time = newChamCong.GioCheckIn });
            }
        }

        // Helper: Hàm tính khoảng cách Euclid giữa 2 vector khuôn mặt
        private double CalculateEuclideanDistance(float[] a, float[] b)
        {
            if (a.Length != b.Length) return double.MaxValue;
            double sum = 0;
            for (int i = 0; i < a.Length; i++)
            {
                sum += Math.Pow(a[i] - b[i], 2);
            }
            return Math.Sqrt(sum);
        }
    }
}