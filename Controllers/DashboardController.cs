using HRApi.Data;
using HRApi.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetDashboardSummary()
        {
            try
            {
                var now = DateTime.Now;
                var result = new DashboardDto();

                // ==========================================
                // 1. Thẻ thông tin cơ bản
                // ==========================================
                result.TongNhanVien = await _context.NhanViens.CountAsync(x => x.TrangThai == true);
                result.TongHopDong = await _context.HopDongs.CountAsync(x => x.TrangThai == "HieuLuc");

                // Hợp đồng sắp hết hạn (trong vòng 30 ngày)
                result.HopDongSapHetHan = await _context.HopDongs
                    .CountAsync(x => x.TrangThai == "HieuLuc" && x.NgayKetThuc.HasValue && x.NgayKetThuc.Value <= now.AddDays(30));

                // Tổng lương kỳ trước (Tháng chốt gần nhất)
                var lastPayroll = await _context.BangLuongs
                    .Where(x => x.DaChot)
                    .OrderByDescending(x => x.Nam).ThenByDescending(x => x.Thang)
                    .FirstOrDefaultAsync();

                if (lastPayroll != null)
                {
                    result.TongLuongKyTruoc = await _context.BangLuongs
                        .Where(x => x.Thang == lastPayroll.Thang && x.Nam == lastPayroll.Nam && x.DaChot)
                        .SumAsync(x => x.ThucLanh);
                }

                // ==========================================
                // 2. Thống kê thâm niên
                // ==========================================
                // FIX: Lấy dữ liệu thô về trước
                var ngayVaoLamList = await _context.NhanViens
                    .Where(x => x.TrangThai == true && x.NgayVaoLam != null)
                    .Select(x => x.NgayVaoLam)
                    .ToListAsync();

                int duoi1Nam = 0, tu1Den3 = 0, tu3Den5 = 0, tu5Den8 = 0, tren8Nam = 0;

                foreach (var date in ngayVaoLamList)
                {
                    var years = (now - date.Value).TotalDays / 365.25;
                    if (years < 1) duoi1Nam++;
                    else if (years < 3) tu1Den3++;
                    else if (years < 5) tu3Den5++;
                    else if (years < 8) tu5Den8++;
                    else tren8Nam++;
                }

                result.ThamNien = new List<ThongKeThamNienDto>
                {
                    new ThongKeThamNienDto { TenThang = "< 1 năm", SoLuong = duoi1Nam },
                    new ThongKeThamNienDto { TenThang = "1-3 năm", SoLuong = tu1Den3 },
                    new ThongKeThamNienDto { TenThang = "3-5 năm", SoLuong = tu3Den5 },
                    new ThongKeThamNienDto { TenThang = "5-8 năm", SoLuong = tu5Den8 },
                    new ThongKeThamNienDto { TenThang = "> 8 năm", SoLuong = tren8Nam }
                };


                // ==========================================
                // 3. Giới tính theo phòng ban 
                // ==========================================
                // Bước A: Lấy dữ liệu thô về RAM (Dùng Include để lấy tên phòng ban)
                var rawPhongBans = await _context.PhongBans
                .Where(p => p.TrangThai == true)
                .Select(p => new
                {
                    TenPhongBan = p.TenPhongBan,
                    // Đếm số lượng nhân viên đang làm việc theo từng giới tính trong phòng này
                    Nam = p.NhanViens.Count(nv => nv.TrangThai == true && nv.GioiTinh == 1),
                    Nu = p.NhanViens.Count(nv => nv.TrangThai == true && nv.GioiTinh == 0),
                    Khac = p.NhanViens.Count(nv => nv.TrangThai == true && nv.GioiTinh != 0 && nv.GioiTinh != 1)
                })
                .ToListAsync();

                result.GioiTinhTheoPhongBan = rawPhongBans.Select(x => new ThongKeGioiTinhDto
                {
                    TenPhongBan = x.TenPhongBan,
                    Nam = x.Nam,
                    Nu = x.Nu,
                    Khac = x.Khac
                }).ToList();


                // ==========================================
                // 4. Lương qua các kỳ (FIX LỖI STRING.FORMAT)
                // ==========================================
                // Bước A: Lấy số liệu thô về RAM
                var rawBangLuongs = await _context.BangLuongs
                    .Where(x => x.DaChot)
                    .Select(x => new
                    {
                        x.Thang,
                        x.Nam,
                        x.ThucLanh
                    })
                    .ToListAsync();

                // Bước B: Group By và Format chuỗi trên RAM
                result.LuongQuaCacKy = rawBangLuongs
                    .GroupBy(x => new { x.Thang, x.Nam })
                    .Select(g => new
                    {
                        Thang = g.Key.Thang,
                        Nam = g.Key.Nam,
                        TongTien = g.Sum(x => x.ThucLanh)
                    })
                    .OrderByDescending(x => x.Nam).ThenByDescending(x => x.Thang)
                    .Take(6)
                    .Select(x => new ThongKeLuongDto
                    {
                        KyLuong = $"Tháng {x.Thang:D2}/{x.Nam}", // C# sẽ xử lý chuỗi này
                        TongTien = x.TongTien
                    })
                    .OrderBy(x => x.KyLuong) // Đảo ngược lại để biểu đồ chạy từ trái sang phải
                    .ToList();


                // ==========================================
                // 5. Đăng ký OT theo phòng ban (FIX LỖI GROUP BY TƯƠNG TỰ)
                // ==========================================
                // Bước A: Lấy dữ liệu thô về RAM
                var rawOTs = await _context.DangKyOTs
                    .Include(x => x.NhanVien).ThenInclude(n => n.PhongBan)
                    .Where(x => x.NgayLamThem.Month == now.Month
                             && x.NgayLamThem.Year == now.Year
                             && x.TrangThai == "Đã duyệt"
                             && x.NhanVien != null
                             && x.NhanVien.PhongBan != null)
                    .Select(x => new
                    {
                        TenPhongBan = x.NhanVien.PhongBan.TenPhongBan,
                        SoGio = x.SoGio
                    })
                    .ToListAsync();

                // Bước B: Group By trên RAM
                result.OTTheoPhongBan = rawOTs
                    .GroupBy(x => x.TenPhongBan)
                    .Select(g => new ThongKeOTDto
                    {
                        TenPhongBan = g.Key,
                        TongSoGio = g.Sum(x => x.SoGio)
                    }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                // Ghi lại lỗi chi tiết để nếu có lỗi khác bạn sẽ thấy ngay trong console Backend
                Console.WriteLine("LỖI DASHBOARD: " + ex.ToString());
                return StatusCode(500, "Lỗi khi xử lý dữ liệu: " + ex.Message);
            }
        }
 
    }
}