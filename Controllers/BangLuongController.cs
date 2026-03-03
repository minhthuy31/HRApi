using HRApi.Data;
using HRApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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

        public class SalaryCalcDto
        {
            public int Year { get; set; }
            public int Month { get; set; }
        }

        // --- 1. TÍNH LƯƠNG (Chỉ Kế toán trưởng & Giám đốc) ---
        [HttpPost("calculate")]
        public async Task<IActionResult> CalculateSalary([FromBody] SalaryCalcDto dto)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Kế toán trưởng" && role != "Giám đốc")
                return StatusCode(403, "Bạn không có quyền thực hiện tính lương.");

            // Check khóa công
            var isAttendanceLocked = await _context.KhoaCongs.AnyAsync(k => k.Nam == dto.Year && k.Thang == dto.Month && k.IsLocked);
            if (!isAttendanceLocked) return BadRequest("Bảng công chưa khóa. Vui lòng yêu cầu Nhân sự khóa công trước.");

            // Check bảng lương đã chốt
            var isPayrollLocked = await _context.BangLuongs.AnyAsync(b => b.Nam == dto.Year && b.Thang == dto.Month && b.DaChot);
            if (isPayrollLocked) return BadRequest("Bảng lương đã chốt. Cần hủy chốt trước khi tính lại.");

            // Xóa dữ liệu cũ chưa chốt
            var oldDrafts = await _context.BangLuongs.Where(b => b.Nam == dto.Year && b.Thang == dto.Month && !b.DaChot).ToListAsync();
            _context.BangLuongs.RemoveRange(oldDrafts);
            await _context.SaveChangesAsync();

            // Lấy danh sách nhân viên
            var employees = await _context.NhanViens
                .Include(n => n.HopDongs)
                .Where(e => e.TrangThai == true)
                .ToListAsync();

            var startDate = new DateTime(dto.Year, dto.Month, 1);
            var endDate = startDate.AddMonths(1);

            // Lấy dữ liệu chấm công
            var attendanceData = await _context.ChamCongs.Where(c => c.NgayChamCong >= startDate && c.NgayChamCong < endDate).ToListAsync();

            // --- [THÊM MỚI] LẤY DỮ LIỆU TĂNG CA (OT) ĐÃ DUYỆT ---
            var otData = await _context.DangKyOTs
                .Where(ot => ot.NgayLamThem >= startDate && ot.NgayLamThem < endDate && ot.TrangThai == "Đã duyệt")
                .GroupBy(ot => ot.MaNhanVien)
                .ToDictionaryAsync(g => g.Key, g => g.Sum(x => x.SoGio));
            // ----------------------------------------------------

            var newPayrolls = new List<BangLuong>();
            decimal standardWorkDays = 26; // Số công chuẩn

            foreach (var emp in employees)
            {
                // Lấy hợp đồng mới nhất
                var activeContract = emp.HopDongs?
                    .OrderByDescending(h => h.NgayBatDau)
                    .FirstOrDefault(h => h.NgayBatDau <= DateTime.Now);

                decimal luongCoBan = activeContract != null ? activeContract.LuongCoBan : emp.LuongCoBan;
                decimal luongDongBH = activeContract != null ? activeContract.LuongCoBan : emp.LuongCoBan; // Hoặc có trường riêng
                decimal phuCap = emp.LuongTroCap;

                // Tính tổng công
                var empAtt = attendanceData.Where(c => c.MaNhanVien == emp.MaNhanVien).ToList();
                double totalWorkDays = empAtt.Sum(c => c.NgayCong);

                // --- [TÍNH TOÁN LƯƠNG CHÍNH] ---
                decimal luongChinh = (luongCoBan / standardWorkDays) * (decimal)totalWorkDays;

                // --- [TÍNH TOÁN LƯƠNG OT] ---
                double totalOtHours = otData.ContainsKey(emp.MaNhanVien) ? otData[emp.MaNhanVien] : 0;

                // Công thức: (Lương cơ bản / 26 / 8) * 150% * Số giờ OT
                // Giả sử làm 8 tiếng/ngày, OT nhân hệ số 1.5
                decimal hourlyRate = (luongCoBan / standardWorkDays) / 8;
                decimal otMultiplier = 1.5m; // Hệ số tăng ca thường
                decimal luongOT = hourlyRate * otMultiplier * (decimal)totalOtHours;
                // -----------------------------

                // Bảo hiểm & Thuế
                decimal bhxh = luongDongBH * 0.08m;
                decimal bhyt = luongDongBH * 0.015m;
                decimal bhtn = luongDongBH * 0.01m;

                decimal tongThuNhap = luongChinh + luongOT + phuCap;
                decimal thuNhapChiuThue = tongThuNhap - (bhxh + bhyt + bhtn) - 11000000; // Giảm trừ bản thân 11tr
                decimal thueTNCN = thuNhapChiuThue > 0 ? thuNhapChiuThue * 0.1m : 0; // Tạm tính 10%
                decimal khoanTruKhac = 0;
                decimal thucLanh = tongThuNhap - (bhxh + bhyt + bhtn) - thueTNCN - khoanTruKhac;

                newPayrolls.Add(new BangLuong
                {
                    MaNhanVien = emp.MaNhanVien,
                    Thang = dto.Month,
                    Nam = dto.Year,
                    LuongCoBan = luongCoBan,
                    LuongDongBaoHiem = luongDongBH,
                    TongPhuCap = phuCap,
                    TongNgayCong = totalWorkDays,

                    // Lưu thông tin OT
                    TongGioOT = totalOtHours,
                    LuongOT = Math.Round(luongOT, 0),

                    LuongChinh = Math.Round(luongChinh, 0),
                    KhauTruBHXH = Math.Round(bhxh, 0),
                    KhauTruBHYT = Math.Round(bhyt, 0),
                    KhauTruBHTN = Math.Round(bhtn, 0),
                    ThueTNCN = Math.Round(thueTNCN, 0),
                    KhoanTruKhac = 0,
                    TongThuNhap = Math.Round(tongThuNhap, 0),
                    ThucLanh = Math.Round(thucLanh, 0),
                    DaChot = false,
                    NgayTinhLuong = DateTime.UtcNow
                });
            }

            await _context.BangLuongs.AddRangeAsync(newPayrolls);
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Đã tính lương cho {newPayrolls.Count} nhân viên." });
        }

        // GET: api/BangLuong?year=2025&month=12
        [HttpGet]
        public async Task<IActionResult> GetPayroll([FromQuery] int year, [FromQuery] int month)
        {
            try
            {
                // 0. Lấy thông tin User hiện tại từ Token một cách linh hoạt
                var role = User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value;
                var deptId = User.FindFirst("MaPhongBan")?.Value;

                // Quét các loại Claim thường chứa ID người dùng
                var currentEmpId = User.FindFirst("MaNhanVien")?.Value
                                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                ?? User.FindFirst("nameid")?.Value
                                ?? User.FindFirst("sub")?.Value;

                // 1. Lấy tất cả nhân viên đang làm việc
                var employees = await _context.NhanViens
                    .Where(nv => nv.TrangThai == true)
                    .ToListAsync();

                // 2. Lấy bảng lương ĐÃ LƯU
                var savedPayrolls = await _context.BangLuongs
                    .Include(b => b.NhanVien)
                    .Where(b => b.Nam == year && b.Thang == month)
                    .ToListAsync();

                // 3. Lấy dữ liệu CHẤM CÔNG
                var attendanceData = await _context.ChamCongs
                    .Where(c => c.NgayChamCong.Year == year && c.NgayChamCong.Month == month)
                    .ToListAsync();

                var attendanceSummary = attendanceData
                    .GroupBy(c => c.MaNhanVien)
                    .ToDictionary(g => g.Key, g => new
                    {
                        TongCong = g.Sum(x => x.NgayCong),
                        NghiCoPhep = g.Count(x => x.NgayCong == 1.0 && !string.IsNullOrEmpty(x.GhiChu) && !x.GhiChu.ToLower().Contains("check-in")),
                        NghiKhongPhep = g.Count(x => x.NgayCong == 0.0),
                        LamNuaNgay = g.Count(x => x.NgayCong == 0.5)
                    });

                var otSummary = await _context.DangKyOTs
                    .Where(ot => ot.NgayLamThem.Year == year && ot.NgayLamThem.Month == month && ot.TrangThai == "Đã duyệt")
                    .GroupBy(ot => ot.MaNhanVien)
                    .ToDictionaryAsync(k => k.Key, v => v.Sum(x => x.SoGio));

                var fullList = new List<BangLuong>();

                // 4. MERGE DỮ LIỆU 
                foreach (var emp in employees)
                {
                    var savedRecord = savedPayrolls.FirstOrDefault(p => p.MaNhanVien == emp.MaNhanVien);

                    double tongCong = attendanceSummary.ContainsKey(emp.MaNhanVien) ? attendanceSummary[emp.MaNhanVien].TongCong : 0;
                    int nghiCoPhep = attendanceSummary.ContainsKey(emp.MaNhanVien) ? attendanceSummary[emp.MaNhanVien].NghiCoPhep : 0;
                    int nghiKP = attendanceSummary.ContainsKey(emp.MaNhanVien) ? attendanceSummary[emp.MaNhanVien].NghiKhongPhep : 0;
                    int lamNua = attendanceSummary.ContainsKey(emp.MaNhanVien) ? attendanceSummary[emp.MaNhanVien].LamNuaNgay : 0;
                    double tongGioOT = otSummary.ContainsKey(emp.MaNhanVien) ? otSummary[emp.MaNhanVien] : 0;

                    if (savedRecord != null)
                    {
                        savedRecord.NhanVien = emp;
                        savedRecord.NghiCoPhep = nghiCoPhep;
                        savedRecord.NghiKhongPhep = nghiKP;
                        savedRecord.LamNuaNgay = lamNua;
                        fullList.Add(savedRecord);
                    }
                    else
                    {
                        fullList.Add(new BangLuong
                        {
                            MaNhanVien = emp.MaNhanVien,
                            NhanVien = emp,
                            Thang = month,
                            Nam = year,
                            LuongCoBan = emp.LuongCoBan,
                            TongPhuCap = emp.LuongTroCap,
                            TongNgayCong = tongCong,
                            TongGioOT = tongGioOT,
                            NghiCoPhep = nghiCoPhep,
                            NghiKhongPhep = nghiKP,
                            LamNuaNgay = lamNua,
                            DaChot = false,
                            ThucLanh = 0
                        });
                    }
                }

                // 5. --- PHÂN QUYỀN LỌC DỮ LIỆU ---
                IEnumerable<BangLuong> finalData = fullList;

                if (role == "Kế toán trưởng" || role == "Giám đốc" || role == "Nhân sự trưởng")
                {
                    // Được xem tất cả, kể cả lúc chưa chốt
                }
                else
                {
                    // Những người khác chỉ xem bảng lương ĐÃ CHỐT
                    finalData = finalData.Where(x => x.DaChot == true);

                    if (role == "Trưởng phòng")
                    {
                        // Trưởng phòng xem phòng mình
                        finalData = finalData.Where(x => x.NhanVien.MaPhongBan == deptId);
                    }
                    else // Nhân viên thường
                    {
                        if (!string.IsNullOrEmpty(currentEmpId))
                        {
                            // Lọc chính xác ID của nhân viên đó
                            finalData = finalData.Where(x =>
                                x.MaNhanVien.Trim().Equals(currentEmpId.Trim(), StringComparison.OrdinalIgnoreCase));
                        }
                        else
                        {
                            // Nếu token lỗi không đọc được ID, trả về rỗng để bảo mật
                            finalData = Enumerable.Empty<BangLuong>();
                        }
                    }
                }

                var isPublished = savedPayrolls.Any(p => p.DaChot);
                decimal departmentTotal = role == "Trưởng phòng" ? finalData.Sum(x => x.ThucLanh) : 0;

                return Ok(new
                {
                    Data = finalData.OrderBy(x => x.MaNhanVien).ToList(),
                    IsPublished = isPublished,
                    DepartmentTotal = departmentTotal
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi tính lương: " + ex.Message);
            }
        }

        // --- 3. LƯU (Chỉ Kế toán & Giám đốc) ---
        [HttpPost("save")]
        public async Task<IActionResult> SavePayroll([FromBody] List<BangLuong> payrollData)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Kế toán trưởng" && role != "Giám đốc")
                return StatusCode(403, "Bạn không có quyền sửa bảng lương.");

            if (payrollData == null || !payrollData.Any()) return BadRequest("Không có dữ liệu.");

            var firstId = payrollData.First().Id;
            var isLocked = await _context.BangLuongs.AnyAsync(b => b.Id == firstId && b.DaChot);

            // Nếu đã chốt, chỉ Giám đốc sửa
            if (isLocked && role != "Giám đốc")
                return BadRequest("Bảng lương đã chốt. Chỉ Kế toán trưởng và Giám đốc mới được sửa.");

            foreach (var item in payrollData)
            {
                var record = await _context.BangLuongs.FindAsync(item.Id);
                if (record != null)
                {
                    record.KhoanTruKhac = item.KhoanTruKhac;
                    decimal cacKhoanTru = record.KhauTruBHXH + record.KhauTruBHYT + record.KhauTruBHTN + record.ThueTNCN + record.KhoanTruKhac;
                    record.ThucLanh = record.TongThuNhap - cacKhoanTru;
                }
            }
            await _context.SaveChangesAsync();
            return Ok(new { message = "Lưu thành công." });
        }

        // --- 4. CHỐT SỔ (Chỉ Kế toán & Giám đốc) ---
        [HttpPost("publish")]
        public async Task<IActionResult> PublishSalary([FromBody] SalaryCalcDto dto, [FromQuery] bool status)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Kế toán trưởng" && role != "Giám đốc")
                return StatusCode(403, "Bạn không có quyền chốt lương.");

            var records = await _context.BangLuongs
                .Where(b => b.Nam == dto.Year && b.Thang == dto.Month)
                .ToListAsync();

            if (!records.Any()) return BadRequest("Không có dữ liệu.");

            foreach (var r in records) r.DaChot = status;
            await _context.SaveChangesAsync();

            return Ok(new { message = status ? "Đã chốt lương." : "Đã hủy chốt lương." });
        }
    }
}