namespace HRApi.DTOs
{
    public class DashboardDto
    {
        // 1. Thẻ Nhân sự & Hợp đồng
        public int TongNhanVien { get; set; }
        public decimal TongLuongKyTruoc { get; set; }
        public int TongHopDong { get; set; }
        public int HopDongSapHetHan { get; set; }

        // 2. Biểu đồ Thâm niên (Donut Chart)
        public List<ThongKeThamNienDto> ThamNien { get; set; } = new List<ThongKeThamNienDto>();

        // 3. Biểu đồ Giới tính theo phòng ban (Bar Chart)
        public List<ThongKeGioiTinhDto> GioiTinhTheoPhongBan { get; set; } = new List<ThongKeGioiTinhDto>();

        // 4. Biểu đồ Lương qua các kỳ (Line Chart)
        public List<ThongKeLuongDto> LuongQuaCacKy { get; set; } = new List<ThongKeLuongDto>();

        // 5. Đăng ký làm thêm theo phòng ban
        public List<ThongKeOTDto> OTTheoPhongBan { get; set; } = new List<ThongKeOTDto>();
    }

    public class ThongKeThamNienDto
    {
        public string TenThang { get; set; } 
        public int SoLuong { get; set; }
    }

    public class ThongKeGioiTinhDto
    {
        public string TenPhongBan { get; set; }
        public int Nam { get; set; }
        public int Nu { get; set; }
        public int Khac { get; set; }
    }

    public class ThongKeLuongDto
    {
        public string KyLuong { get; set; }
        public decimal TongTien { get; set; }
    }

    public class ThongKeOTDto
    {
        public string TenPhongBan { get; set; }
        public double TongSoGio { get; set; }
    }
}