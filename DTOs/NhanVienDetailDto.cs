namespace HRApi.DTOs
{
    public class NhanVienDetailDto
    {
        public string MaNhanVien { get; set; }

        // 1. Thông tin cơ bản
        public string? HoTen { get; set; }
        public bool TrangThai { get; set; }
        public string? HinhAnh { get; set; }

        // 2. Thông tin cá nhân
        public DateTime? NgaySinh { get; set; }
        public int? GioiTinh { get; set; }
        public string? DanToc { get; set; }
        public string? TonGiao { get; set; }
        public string? QueQuan { get; set; }
        public string? NoiSinh { get; set; }
        public string? QuocTich { get; set; }
        public string? TinhTrangHonNhan { get; set; }

        // 3. Giấy tờ
        public string? CCCD { get; set; }
        public DateTime? NgayCapCCCD { get; set; }
        public string? NoiCapCCCD { get; set; }
        public DateTime? NgayHetHanCCCD { get; set; }

        public string? SoHoChieu { get; set; }
        public DateTime? NgayCapHoChieu { get; set; }
        public DateTime? NgayHetHanHoChieu { get; set; }
        public string? NoiCapHoChieu { get; set; }

        // 4. Liên hệ
        public string? Email { get; set; }
        public string? sdt_NhanVien { get; set; }

        public string? NguoiLienHeKhanCap { get; set; }
        public string? SdtKhanCap { get; set; }
        public string? QuanHeKhanCap { get; set; }
        public string? DiaChiKhanCap { get; set; }

        public string? DiaChiThuongTru { get; set; }
        public string? PhuongXaThuongTru { get; set; }
        public string? QuanHuyenThuongTru { get; set; }
        public string? TinhThanhThuongTru { get; set; }
        public string? QuocGiaThuongTru { get; set; }
        public string? DiaChiTamTru { get; set; }

        // 5. Công việc & Lương (Đã cập nhật)
        public DateTime? NgayVaoLam { get; set; }
        public DateTime? NgayNghiViec { get; set; }
        public string? LoaiNhanVien { get; set; }

        // --- THÊM 2 TRƯỜNG MỚI ---
        public decimal LuongCoBan { get; set; }
        public decimal LuongTroCap { get; set; }
        public string? SoHopDong { get; set; }

        public string? MaQuanLyTrucTiep { get; set; }
        public string? TenQuanLyTrucTiep { get; set; }

        public string? MaPhongBan { get; set; }
        public string? MaChucVuNV { get; set; }
        public int? RoleId { get; set; }

        // 6. Học vấn
        public string? MaTrinhDoHocVan { get; set; }
        public string? MaChuyenNganh { get; set; }
        public string? NoiDaoTao { get; set; }
        public string? HeDaoTao { get; set; }
        public string? ChuyenNganhChiTiet { get; set; }

        // 7. Ngân hàng
        public string? TenNganHang { get; set; }
        public string? SoTaiKhoanNH { get; set; }
        public string? TenTaiKhoanNH { get; set; }

        // 8. Bảo hiểm
        public string? SoBHYT { get; set; }
        public string? SoBHXH { get; set; }
        public string? NoiDKKCB { get; set; }

        // Thông tin liên kết (Display Names)
        public string? TenPhongBan { get; set; }
        public string? TenChucVu { get; set; }
        public string? TenChuyenNganh { get; set; }
        public string? TenTrinhDoHocVan { get; set; }
        public string? TenRole { get; set; }
    }
}