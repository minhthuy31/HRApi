using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRApi.Models
{
    public class NhanVien
    {
        // ========================================================================
        // 1. THÔNG TIN CƠ BẢN & ĐĂNG NHẬP
        // ========================================================================
        [Key]
        public string MaNhanVien { get; set; } // Mã NV (Vừa là khóa chính, vừa là mã hiển thị)

        public string MatKhau { get; set; } // Mật khẩu (đã hash)
        public bool TrangThai { get; set; } = true; // true: Đang làm, false: Đã nghỉ
        public string? HinhAnh { get; set; } // URL ảnh đại diện

        // Reset Password
        public string? ResetCode { get; set; }
        public DateTime? ResetCodeExpiry { get; set; }


        // ========================================================================
        // 2. THÔNG TIN CÁ NHÂN (Personal Info)
        // ========================================================================
        [Required]
        public string? HoTen { get; set; }
        public DateTime? NgaySinh { get; set; }
        public int? GioiTinh { get; set; } // 0: Nữ, 1: Nam
        public string? DanToc { get; set; }
        public string? TonGiao { get; set; } // Thêm Tôn giáo (thường đi kèm Dân tộc)
        public string? QueQuan { get; set; } // Quê quán (trên giấy tờ)
        public string? NoiSinh { get; set; } // Nơi sinh thực tế
        public string? QuocTich { get; set; } // Quốc tịch
        public string? TinhTrangHonNhan { get; set; } // Độc thân, Đã kết hôn...


        // ========================================================================
        // 3. GIẤY TỜ TÙY THÂN (Identity)
        // ========================================================================
        // --- CCCD/CMND ---
        public string? CCCD { get; set; }
        public DateTime? NgayCapCCCD { get; set; }
        public string? NoiCapCCCD { get; set; }
        public DateTime? NgayHetHanCCCD { get; set; } // Ngày hết hạn CCCD

        // --- Hộ chiếu (Passport) ---
        public string? SoHoChieu { get; set; }
        public DateTime? NgayCapHoChieu { get; set; }
        public DateTime? NgayHetHanHoChieu { get; set; }
        public string? NoiCapHoChieu { get; set; } // Quốc gia/Nơi cấp


        // ========================================================================
        // 4. THÔNG TIN LIÊN HỆ (Contact)
        // ========================================================================
        public string? Email { get; set; } // Email cá nhân/công ty
        public string? sdt_NhanVien { get; set; } // SĐT chính

        // --- Liên hệ khẩn cấp ---
        public string? NguoiLienHeKhanCap { get; set; } // Tên người thân
        public string? SdtKhanCap { get; set; } // SĐT người thân
        public string? QuanHeKhanCap { get; set; } // Mối quan hệ (Bố, Mẹ, Vợ...)
        public string? DiaChiKhanCap { get; set; }

        // --- Địa chỉ thường trú (Hộ khẩu) ---
        public string? DiaChiThuongTru { get; set; } // Địa chỉ chi tiết
        public string? PhuongXaThuongTru { get; set; }
        public string? QuanHuyenThuongTru { get; set; }
        public string? TinhThanhThuongTru { get; set; }
        public string? QuocGiaThuongTru { get; set; }

        // --- Địa chỉ tạm trú (Nơi ở hiện tại) ---
        public string? DiaChiTamTru { get; set; } // Địa chỉ chi tiết nơi đang ở


        // ========================================================================
        // 5. QUÁ TRÌNH LÀM VIỆC & CÔNG VIỆC (Job Info)
        // ========================================================================
        public DateTime? NgayVaoLam { get; set; } // Ngày bắt đầu làm việc
        public DateTime? NgayNghiViec { get; set; } // Ngày nghỉ việc (nếu có)
        public string? LoaiNhanVien { get; set; } // Full-time, Part-time, CTV...

        // --- BỔ SUNG: THÔNG TIN LƯƠNG & HỢP ĐỒNG ---
        [Column(TypeName = "decimal(18, 2)")]
        public decimal LuongCoBan { get; set; } // Lương cơ bản hiện tại
        public decimal LuongTroCap { get; set; } // Tổng lương trợ cấp hiện tại
        public string? SoHopDong { get; set; }  // Số hợp đồng hiện tại

        // Quản lý trực tiếp (Self-referencing Foreign Key)
        public string? MaQuanLyTrucTiep { get; set; }
        [ForeignKey("MaQuanLyTrucTiep")]
        public virtual NhanVien? QuanLyTrucTiep { get; set; }

        // Các khóa ngoại (Phòng ban, Chức vụ...)
        public string? MaPhongBan { get; set; }
        [ForeignKey("MaPhongBan")]
        public virtual PhongBan? PhongBan { get; set; }

        public string? MaChucVuNV { get; set; }
        [ForeignKey("MaChucVuNV")]
        public virtual ChucVuNhanVien? ChucVuNhanVien { get; set; }

        public int? RoleId { get; set; }
        [ForeignKey("RoleId")]
        public virtual UserRole? UserRole { get; set; }


        // ========================================================================
        // 6. TRÌNH ĐỘ HỌC VẤN (Education)
        // ========================================================================
        // Vẫn giữ khóa ngoại tới bảng TrinhDoHocVan để phân loại chung (Cử nhân, Thạc sĩ...)
        public string? MaTrinhDoHocVan { get; set; }
        [ForeignKey("MaTrinhDoHocVan")]
        public virtual TrinhDoHocVan? TrinhDoHocVan { get; set; }

        public string? MaChuyenNganh { get; set; }
        [ForeignKey("MaChuyenNganh")]
        public virtual ChuyenNganh? ChuyenNganh { get; set; }

        // Chi tiết đào tạo
        public string? NoiDaoTao { get; set; } // Đơn vị đào tạo (Đại học Bách Khoa...)
        public string? HeDaoTao { get; set; } // Chính quy, Tại chức...
        public string? ChuyenNganhChiTiet { get; set; } // Tên chuyên ngành cụ thể trên bằng


        // ========================================================================
        // 7. THÔNG TIN TÀI KHOẢN NGÂN HÀNG (Bank Info)
        // ========================================================================
        public string? TenNganHang { get; set; } // Vietcombank, Techcombank...
        public string? SoTaiKhoanNH { get; set; }
        public string? TenTaiKhoanNH { get; set; } // NGUYEN VAN A (Thường viết hoa không dấu)


        // ========================================================================
        // 8. BẢO HIỂM (Insurance)
        // ========================================================================
        public string? SoBHYT { get; set; } // Số bảo hiểm y tế
        public string? SoBHXH { get; set; } // Số bảo hiểm xã hội
        public string? NoiDKKCB { get; set; } // Nơi đăng ký khám chữa bệnh ban đầu


        // ========================================================================
        // 9. QUAN HỆ HỢP ĐỒNG (Navigation)
        // ========================================================================
        // Danh sách lịch sử hợp đồng của nhân viên này
        public virtual ICollection<HopDong> HopDongs { get; set; }
        // Lưu chuỗi Base64. Dùng nvarchar(MAX) trong SQL Server
        [Column(TypeName = "nvarchar(MAX)")]
        public string? ChuKy { get; set; }
    }
}