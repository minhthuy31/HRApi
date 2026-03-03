namespace HRApi.DTOs
{
    public class HopDongDetailDto
    {
        public string SoHopDong { get; set; }
        public string LoaiHopDong { get; set; }
        public DateTime NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public string TrangThai { get; set; }
        public decimal LuongCoBan { get; set; }
        public string? TepDinhKem { get; set; }
        public string? GhiChu { get; set; }
    }
}
