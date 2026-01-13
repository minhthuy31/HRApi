using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRApi.Models
{
    public class BangLuong
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int Thang { get; set; }
        [Required]
        public int Nam { get; set; }

        // --- 1. CÁC KHOẢN THU NHẬP (Lưu snapshot từ Hợp đồng sang) ---
        [Column(TypeName = "decimal(18,2)")]
        public decimal LuongCoBan { get; set; } // Lương cứng

        [Column(TypeName = "decimal(18,2)")]
        public decimal LuongDongBaoHiem { get; set; } // Mức lương dùng để đóng bảo hiểm

        [Column(TypeName = "decimal(18,2)")]
        public decimal TongPhuCap { get; set; } // Tổng các loại phụ cấp (Lương trợ cấp)

        // --- 2. SỐ LIỆU TỪ CHẤM CÔNG ---
        public double TongNgayCong { get; set; } // Tổng số công thực tế
        public double TongGioOT { get; set; }    // Tổng số giờ OT

        // --- [MỚI] THUỘC TÍNH CHỈ HIỂN THỊ (Không lưu DB) ---
        // Dùng để Frontend hiển thị chi tiết
        [NotMapped]
        public int NghiCoPhep { get; set; }
        [NotMapped]
        public int NghiKhongPhep { get; set; }
        [NotMapped]
        public int LamNuaNgay { get; set; }

        // --- 3. TÍNH TOÁN CHI TIẾT ---
        [Column(TypeName = "decimal(18,2)")]
        public decimal LuongChinh { get; set; } // Lương theo ngày công

        [Column(TypeName = "decimal(18,2)")]
        public decimal LuongOT { get; set; }    // Lương làm thêm giờ

        // --- 4. CÁC KHOẢN KHẤU TRỪ (BẢO HIỂM & THUẾ) ---
        [Column(TypeName = "decimal(18,2)")]
        public decimal KhauTruBHXH { get; set; } // 8%

        [Column(TypeName = "decimal(18,2)")]
        public decimal KhauTruBHYT { get; set; } // 1.5%

        [Column(TypeName = "decimal(18,2)")]
        public decimal KhauTruBHTN { get; set; } // 1%

        [Column(TypeName = "decimal(18,2)")]
        public decimal ThueTNCN { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal KhoanTruKhac { get; set; } // Phạt, tạm ứng... (Cho phép sửa)

        // --- 5. TỔNG KẾT ---
        [Column(TypeName = "decimal(18,2)")]
        public decimal TongThuNhap { get; set; } // Gross

        [Column(TypeName = "decimal(18,2)")]
        public decimal ThucLanh { get; set; }    // Net (Thực nhận)

        // --- TRẠNG THÁI ---
        public DateTime NgayTinhLuong { get; set; } = DateTime.UtcNow;
        public bool DaChot { get; set; } = false; // true: Đã chốt sổ
        public string? GhiChu { get; set; }

        // --- KHÓA NGOẠI ---
        [Required]
        [ForeignKey("NhanVien")]
        public string MaNhanVien { get; set; }
        public virtual NhanVien? NhanVien { get; set; }
    }
}