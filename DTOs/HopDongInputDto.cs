namespace HRApi.DTOs
{
    public class HopDongInputDto
    {
        public string SoHopDong { get; set; }
        public string MaNhanVien { get; set; }
        public string LoaiHopDong { get; set; }
        public DateTime NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public decimal LuongCoBan { get; set; }
        public IFormFile? FileDinhKem { get; set; }
        public string? GhiChu { get; set; }
        public string TrangThai { get; set; } = "HieuLuc";
    }
}
