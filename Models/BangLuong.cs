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
        // Lưu lại mức lương tại thời điểm tính, phòng trường hợp sau này hợp đồng thay đổi
        [Column(TypeName = "decimal(18,2)")]
        public decimal LuongCoBan { get; set; } // Lương cứng

        [Column(TypeName = "decimal(18,2)")]
        public decimal LuongDongBaoHiem { get; set; } // Mức lương dùng để đóng bảo hiểm

        [Column(TypeName = "decimal(18,2)")]
        public decimal TongPhuCap { get; set; } // Tổng các loại phụ cấp (Trách nhiệm + Ăn trưa + Khác...)

        // --- 2. SỐ LIỆU TỪ CHẤM CÔNG ---
        public double TongNgayCong { get; set; } // Tổng số công thường (bao gồm cả công tác)
        public double TongGioOT { get; set; }    // Tổng số giờ OT

        // --- 3. TÍNH TOÁN CHI TIẾT ---
        [Column(TypeName = "decimal(18,2)")]
        public decimal LuongChinh { get; set; } // = (Lương CB + Phụ cấp) / 26 * TongNgayCong

        [Column(TypeName = "decimal(18,2)")]
        public decimal LuongOT { get; set; }    // = (Lương CB / 26 / 8) * 1.5 * TongGioOT

        // --- 4. CÁC KHOẢN KHẤU TRỪ (BẢO HIỂM & THUẾ) ---
        // Tỷ lệ hiện tại (tham khảo): BHXH (8%), BHYT (1.5%), BHTN (1%)
        [Column(TypeName = "decimal(18,2)")]
        public decimal KhauTruBHXH { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal KhauTruBHYT { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal KhauTruBHTN { get; set; } // Bảo hiểm thất nghiệp

        [Column(TypeName = "decimal(18,2)")]
        public decimal ThueTNCN { get; set; }    // Thuế thu nhập cá nhân

        [Column(TypeName = "decimal(18,2)")]
        public decimal KhoanTruKhac { get; set; } // Phạt đi muộn, tạm ứng...

        // --- 5. TỔNG KẾT ---
        [Column(TypeName = "decimal(18,2)")]
        public decimal TongThuNhap { get; set; } // Tổng thu nhập trước thuế/BH (Gross)

        [Column(TypeName = "decimal(18,2)")]
        public decimal ThucLanh { get; set; }    // Số tiền thực nhận về tay (Net)

        // --- TRẠNG THÁI ---
        public DateTime NgayTinhLuong { get; set; } = DateTime.UtcNow;
        public bool DaChot { get; set; } = false; // true: Đã chốt sổ, không sửa được nữa
        public string? GhiChu { get; set; }

        // --- KHÓA NGOẠI ---
        [Required]
        [ForeignKey("NhanVien")]
        public string MaNhanVien { get; set; }
        public virtual NhanVien NhanVien { get; set; }
    }
}