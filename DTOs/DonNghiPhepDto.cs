namespace HRApi.DTOs
{
    public class DonNghiPhepDto
    {
        public int Id { get; set; }
        public string MaNhanVien { get; set; }
        public string HoTenNhanVien { get; set; }
        public DateTime NgayNghi { get; set; }
        public DateTime NgayGuiDon { get; set; }
        public string LyDo { get; set; }
        public string TrangThai { get; set; }
    }
}
