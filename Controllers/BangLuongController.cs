using HRApi.Data;
using HRApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        // GET: api/BangLuong?year=2025&month=12
        [HttpGet]
        public async Task<IActionResult> GetPayroll([FromQuery] int year, [FromQuery] int month)
        {
            try
            {
                // 1. Lấy tất cả nhân viên đang làm việc
                var employees = await _context.NhanViens
                    .Where(nv => nv.TrangThai == true)
                    .ToListAsync();

                // 2. Lấy bảng lương ĐÃ LƯU (nếu có)
                var savedPayrolls = await _context.BangLuongs
                    .Include(b => b.NhanVien)
                    .Where(b => b.Nam == year && b.Thang == month)
                    .ToListAsync();

                // 3. Lấy dữ liệu CHẤM CÔNG
                var attendanceData = await _context.ChamCongs
                    .Where(c => c.NgayChamCong.Year == year && c.NgayChamCong.Month == month)
                    .ToListAsync();

                // --- [SỬA LOGIC ĐẾM NGÀY NGHỈ TẠI ĐÂY] ---
                var attendanceSummary = attendanceData
                    .GroupBy(c => c.MaNhanVien)
                    .ToDictionary(g => g.Key, g => new
                    {
                        TongCong = g.Sum(x => x.NgayCong),

                        // --- LOGIC MỚI CHO NGHỈ CÓ PHÉP ---
                        // Điều kiện: Ngày công = 1.0 (được hưởng lương)
                        // VÀ Ghi chú có từ khóa liên quan đến nghỉ (Phép, Ốm, Việc riêng...)
                        // VÀ Ghi chú KHÔNG chứa "Check-in" (để tránh đếm nhầm ngày đi làm)
                        NghiCoPhep = g.Count(x => x.NgayCong == 1.0 &&
                                                  !string.IsNullOrEmpty(x.GhiChu) &&
                                                  !x.GhiChu.ToLower().Contains("check-in") && // Loại trừ ngày đi làm thường
                                                  (
                                                      x.GhiChu.ToLower().Contains("phép") ||
                                                      x.GhiChu.ToLower().Contains("nghỉ") ||
                                                      x.GhiChu.ToLower().Contains("việc riêng") ||
                                                      x.GhiChu.ToLower().Contains("ốm") ||
                                                      x.GhiChu.ToLower().Contains("thai sản") ||
                                                      x.GhiChu.Trim().ToLower() == "p" // Trường hợp viết tắt
                                                  )),

                        // --- LOGIC NGHỈ KHÔNG PHÉP (Giữ nguyên hoặc mở rộng) ---
                        // Điều kiện: Ngày công = 0.0
                        // VÀ (Ghi chú chứa từ khóa Vắng/KP HOẶC Ghi chú rỗng)
                        NghiKhongPhep = g.Count(x => x.NgayCong == 0.0 &&
                                                     (
                                                        string.IsNullOrEmpty(x.GhiChu) ||
                                                        x.GhiChu.ToLower().Contains("không phép") ||
                                                        x.GhiChu.ToLower().Contains("vắng") ||
                                                        x.GhiChu.ToLower().Contains("kp")
                                                     )),

                        LamNuaNgay = g.Count(x => x.NgayCong == 0.5)
                    });

                // 4. Lấy dữ liệu OT
                var otSummary = await _context.DangKyOTs
                    .Where(ot => ot.NgayLamThem.Year == year && ot.NgayLamThem.Month == month && ot.TrangThai == "Đã duyệt")
                    .GroupBy(ot => ot.MaNhanVien)
                    .ToDictionaryAsync(k => k.Key, v => v.Sum(x => x.SoGio));

                var result = new List<BangLuong>();

                foreach (var emp in employees)
                {
                    var savedRecord = savedPayrolls.FirstOrDefault(p => p.MaNhanVien == emp.MaNhanVien);

                    // Lấy số liệu chấm công hiện tại
                    double tongCong = 0;
                    int nghiCoPhep = 0;
                    int nghiKhongPhep = 0;
                    int lamNuaNgay = 0;

                    if (attendanceSummary.ContainsKey(emp.MaNhanVien))
                    {
                        var att = attendanceSummary[emp.MaNhanVien];
                        tongCong = att.TongCong;
                        nghiCoPhep = att.NghiCoPhep;
                        nghiKhongPhep = att.NghiKhongPhep;
                        lamNuaNgay = att.LamNuaNgay;
                    }

                    double tongGioOT = otSummary.ContainsKey(emp.MaNhanVien) ? otSummary[emp.MaNhanVien] : 0;

                    // Lấy lương từ hồ sơ (Ưu tiên cập nhật mới nếu chưa chốt sổ)
                    decimal luongCoBan = emp.LuongCoBan;
                    decimal luongTroCap = emp.LuongTroCap;

                    if (savedRecord != null && savedRecord.DaChot)
                    {
                        luongCoBan = savedRecord.LuongCoBan;
                        luongTroCap = savedRecord.TongPhuCap;
                    }

                    // --- TÍNH TOÁN ---
                    decimal luongChinh = (luongCoBan / 26m) * (decimal)tongCong;
                    decimal donGiaGio = (luongCoBan / 26m / 8m);
                    decimal luongOT = donGiaGio * 1.5m * (decimal)tongGioOT;

                    decimal bhxh = luongCoBan * 0.08m;
                    decimal bhyt = luongCoBan * 0.015m;
                    decimal bhtn = luongCoBan * 0.01m;

                    decimal tongThuNhap = luongChinh + luongOT + luongTroCap;
                    decimal khoanTruKhac = savedRecord != null ? savedRecord.KhoanTruKhac : 0;
                    decimal thucLanh = tongThuNhap - (bhxh + bhyt + bhtn) - khoanTruKhac;

                    if (savedRecord != null)
                    {
                        // Cập nhật bản ghi cũ (nếu chưa chốt)
                        if (!savedRecord.DaChot)
                        {
                            savedRecord.LuongCoBan = luongCoBan;
                            savedRecord.TongPhuCap = luongTroCap;
                            savedRecord.TongNgayCong = tongCong;
                            savedRecord.TongGioOT = tongGioOT;

                            savedRecord.LuongChinh = Math.Round(luongChinh, 0);
                            savedRecord.LuongOT = Math.Round(luongOT, 0);
                            savedRecord.KhauTruBHXH = Math.Round(bhxh, 0);
                            savedRecord.KhauTruBHYT = Math.Round(bhyt, 0);
                            savedRecord.KhauTruBHTN = Math.Round(bhtn, 0);
                            savedRecord.TongThuNhap = Math.Round(tongThuNhap, 0);
                            savedRecord.ThucLanh = Math.Round(thucLanh, 0);
                        }

                        // Gán lại thông tin hiển thị
                        savedRecord.NhanVien = emp;
                        savedRecord.NghiCoPhep = nghiCoPhep;
                        savedRecord.NghiKhongPhep = nghiKhongPhep;
                        savedRecord.LamNuaNgay = lamNuaNgay;

                        result.Add(savedRecord);
                    }
                    else
                    {
                        // Tạo mới (Preview)
                        var draftPayroll = new BangLuong
                        {
                            MaNhanVien = emp.MaNhanVien,
                            NhanVien = emp,
                            Thang = month,
                            Nam = year,
                            LuongCoBan = luongCoBan,
                            TongPhuCap = luongTroCap,

                            TongNgayCong = tongCong,
                            TongGioOT = tongGioOT,
                            NghiCoPhep = nghiCoPhep,
                            NghiKhongPhep = nghiKhongPhep,
                            LamNuaNgay = lamNuaNgay,

                            LuongChinh = Math.Round(luongChinh, 0),
                            LuongOT = Math.Round(luongOT, 0),
                            KhauTruBHXH = Math.Round(bhxh, 0),
                            KhauTruBHYT = Math.Round(bhyt, 0),
                            KhauTruBHTN = Math.Round(bhtn, 0),
                            TongThuNhap = Math.Round(tongThuNhap, 0),
                            ThucLanh = Math.Round(thucLanh, 0),
                            KhoanTruKhac = 0,
                            ThueTNCN = 0
                        };
                        result.Add(draftPayroll);
                    }
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi tính lương: " + ex.Message);
            }
        }

        // POST: api/BangLuong/save (Giữ nguyên)
        [HttpPost("save")]
        public async Task<IActionResult> SavePayroll([FromBody] List<BangLuong> payrollData)
        {
            if (payrollData == null || !payrollData.Any()) return BadRequest("Không có dữ liệu.");

            var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var record in payrollData)
                {
                    var existing = await _context.BangLuongs.AsNoTracking()
                        .FirstOrDefaultAsync(b => b.MaNhanVien == record.MaNhanVien && b.Thang == record.Thang && b.Nam == record.Nam);

                    record.NgayTinhLuong = DateTime.UtcNow;
                    record.NhanVien = null;

                    if (existing != null)
                    {
                        record.Id = existing.Id;
                        _context.BangLuongs.Update(record);
                    }
                    else
                    {
                        record.Id = 0;
                        _context.BangLuongs.Add(record);
                    }
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(new { message = "Lưu thành công." });
            }
            catch (Exception ex) { await transaction.RollbackAsync(); return StatusCode(500, ex.Message); }
        }
    }
}