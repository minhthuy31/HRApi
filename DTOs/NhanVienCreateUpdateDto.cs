namespace HRApi.DTOs
{

    public class NhanVienCreateUpdateDto
    {
        // 1. Thông tin cơ bản
        public string? HoTen { get; set; }
        public string? MatKhau { get; set; } // Optional khi update
        public bool TrangThai { get; set; } = true;
        public string? HinhAnh { get; set; }

        // 2. Thông tin cá nhân
        public DateTime? NgaySinh { get; set; }
        public int? GioiTinh { get; set; }
        public string? DanToc { get; set; }
        public string? TonGiao { get; set; }      // New
        public string? QueQuan { get; set; }
        public string? NoiSinh { get; set; }      // New
        public string? QuocTich { get; set; }     // New
        public string? TinhTrangHonNhan { get; set; }

        // 3. Giấy tờ tùy thân
        public string? CCCD { get; set; }
        public DateTime? NgayCapCCCD { get; set; }
        public string? NoiCapCCCD { get; set; }
        public DateTime? NgayHetHanCCCD { get; set; } // New

        // Hộ chiếu (New)
        public string? SoHoChieu { get; set; }
        public DateTime? NgayCapHoChieu { get; set; }
        public DateTime? NgayHetHanHoChieu { get; set; }
        public string? NoiCapHoChieu { get; set; }

        // 4. Liên hệ
        public string? Email { get; set; }
        public string? sdt_NhanVien { get; set; }

        // Liên hệ khẩn cấp (New)
        public string? NguoiLienHeKhanCap { get; set; }
        public string? SdtKhanCap { get; set; }
        public string? QuanHeKhanCap { get; set; }
        public string? DiaChiKhanCap { get; set; }

        // Địa chỉ thường trú (Full)
        public string? DiaChiThuongTru { get; set; }
        public string? PhuongXaThuongTru { get; set; }
        public string? QuanHuyenThuongTru { get; set; }
        public string? TinhThanhThuongTru { get; set; }
        public string? QuocGiaThuongTru { get; set; }

        public string? DiaChiTamTru { get; set; }

        // 5. Công việc
        public DateTime? NgayVaoLam { get; set; }   // New
        public DateTime? NgayNghiViec { get; set; } // New
        public string? LoaiNhanVien { get; set; }
        public string? MaQuanLyTrucTiep { get; set; } // New

        public string? MaPhongBan { get; set; }
        public string? MaChucVuNV { get; set; }
        // RoleId thường được gán tự động từ Chức vụ trong Controller, nhưng có thể truyền nếu cần override
        public int? RoleId { get; set; }

        // 6. Học vấn
        public string? MaTrinhDoHocVan { get; set; }
        public string? MaChuyenNganh { get; set; }
        public string? NoiDaoTao { get; set; }        // New
        public string? HeDaoTao { get; set; }         // New
        public string? ChuyenNganhChiTiet { get; set; } // New

        // 7. Ngân hàng
        public string? TenNganHang { get; set; }
        public string? SoTaiKhoanNH { get; set; }
        public string? TenTaiKhoanNH { get; set; }    // New

        // 8. Bảo hiểm (New)
        public string? SoBHYT { get; set; }
        public string? SoBHXH { get; set; }
        public string? NoiDKKCB { get; set; }
    }
}