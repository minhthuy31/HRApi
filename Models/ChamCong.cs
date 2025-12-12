using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRApi.Models
{
    public class ChamCong
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime NgayChamCong { get; set; } // Ngày chấm công (phần giờ set về 00:00:00)

        // --- GIỜ GIẤC ---
        public DateTime? GioCheckIn { get; set; }  // Giờ vào thực tế
        public DateTime? GioCheckOut { get; set; } // Giờ ra thực tế

        // --- TÍNH TOÁN CÔNG ---
        public double NgayCong { get; set; } = 0; // 1.0 (Full), 0.5 (Nửa), 0.0 (Nghỉ)

        public double SoGioLamViec { get; set; } = 0; // Tổng giờ làm việc hành chính
        public double SoGioOT { get; set; } = 0;      // Tổng giờ tăng ca (đã duyệt)

        // --- PHÂN LOẠI & TRẠNG THÁI ---
        // Loại ngày công: "Làm việc", "Nghỉ phép", "Nghỉ không phép", "Công tác", "Lễ tết"
        public string LoaiNgayCong { get; set; } = "Làm việc";

        public bool DiMuon { get; set; } = false; // Cờ báo đi muộn (để báo cáo)
        public bool VeSom { get; set; } = false;  // Cờ báo về sớm (để báo cáo)

        public string? GhiChu { get; set; } // Lý do chỉnh sửa hoặc ghi chú từ đơn

        // --- KHÓA NGOẠI ---
        [Required]
        [ForeignKey("NhanVien")]
        public string MaNhanVien { get; set; }
        public virtual NhanVien NhanVien { get; set; }
    }
}