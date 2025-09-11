namespace HRApi.DTOs
{
    public class NhanVienDetailDto
    {
        public string MaNhanVien { get; set; }
        public string? HoTen { get; set; }
        public DateTime? NgaySinh { get; set; }
        public int? GioiTinh { get; set; }
        public string? DanToc { get; set; }
        public string? TinhTrangHonNhan { get; set; }
        public string? QueQuan { get; set; }
        public string? DiaChiThuongTru { get; set; }
        public string? DiaChiTamTru { get; set; }
        public string? HinhAnh { get; set; }
        public string? sdt_NhanVien { get; set; }
        public string? Email { get; set; }
        public string? CCCD { get; set; }
        public DateTime? NgayCapCCCD { get; set; }
        public string? NoiCapCCCD { get; set; }
        public string? LoaiNhanVien { get; set; }
        public bool TrangThai { get; set; }
        public string? SoTaiKhoanNH { get; set; }
        public string? TenNganHang { get; set; }

        public string? MaPhongBan { get; set; }
        public string? MaChucVuNV { get; set; }
        public string? MaHopDong { get; set; }
        public string? MaChuyenNganh { get; set; }
        public string? MaTrinhDoHocVan { get; set; }

        public string? TenPhongBan { get; set; }
        public string? TenChucVu { get; set; }
        public string? TenChuyenNganh { get; set; }
        public string? TenTrinhDoHocVan { get; set; }
        public string? TenRole { get; set; }
        public int? RoleId { get; set; }
    }
}