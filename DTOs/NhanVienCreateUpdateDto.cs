namespace HRApi.DTOs
{
    public class NhanVienCreateUpdateDto
    {
        public string? HoTen { get; set; }
        public string? MatKhau { get; set; }
        public DateTime? NgaySinh { get; set; }
        public int? GioiTinh { get; set; }
        public string? DanToc { get; set; }
        public string? TinhTrangHonNhan { get; set; } // Thêm trường mới
        public string? QueQuan { get; set; }
        public string? DiaChiThuongTru { get; set; } // Thêm trường mới
        public string? DiaChiTamTru { get; set; } // Thêm trường mới
        public string? HinhAnh { get; set; }
        public string? sdt_NhanVien { get; set; }
        public string? Email { get; set; } // Thêm trường mới
        public string? CCCD { get; set; }
        public DateTime? NgayCapCCCD { get; set; } // Thêm trường mới
        public string? NoiCapCCCD { get; set; } // Thêm trường mới
        public string? LoaiNhanVien { get; set; } // Thêm trường mới
        public bool TrangThai { get; set; }
        public string? SoTaiKhoanNH { get; set; } // Thêm trường mới
        public string? TenNganHang { get; set; } // Thêm trường mới
        public string? MaChucVuNV { get; set; }
        public string? MaPhongBan { get; set; }
        public string? MaHopDong { get; set; }
        public string? MaChuyenNganh { get; set; }
        public string? MaTrinhDoHocVan { get; set; }
        public int? RoleId { get; set; }
    }
}