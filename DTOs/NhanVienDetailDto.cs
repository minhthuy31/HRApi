// HRApi/DTOs/NhanVienDetailDto.cs

namespace HRApi.DTOs
{
    // DTO để lấy thông tin chi tiết của nhân viên
    public class NhanVienDetailDto
    {
        public string MaNhanVien { get; set; }
        public string? HoTen { get; set; }
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

        // Các mã khóa ngoại
        public string? MaPhongBan { get; set; }
        public string? MaChucVuNV { get; set; }
        public string? MaHopDong { get; set; }
        public string? MaChuyenNganh { get; set; }
        public string? MaTrinhDoHocVan { get; set; }

        // Tên từ các bảng liên quan
        public string? TenPhongBan { get; set; }
        public string? TenChucVu { get; set; }
        public string? TenChuyenNganh { get; set; }
        public string? TenTrinhDoHocVan { get; set; }
    }
}