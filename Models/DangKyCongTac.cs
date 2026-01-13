using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRApi.Models
{
    public class DangKyCongTac
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string MaNhanVien { get; set; }
        [ForeignKey("MaNhanVien")]
        public NhanVien? NhanVien { get; set; }

        [Required]
        public DateTime NgayBatDau { get; set; }

        [Required]
        public DateTime NgayKetThuc { get; set; }

        [Required]
        public string NoiCongTac { get; set; }

        [Required]
        public string MucDich { get; set; }

        public string? PhuongTien { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal KinhPhiDuKien { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal SoTienTamUng { get; set; } = 0; // Nếu = 0 nghĩa là nhân viên tự ứng

        public string? LyDoTamUng { get; set; }

        public string TrangThai { get; set; } = "Chờ duyệt"; // Chờ duyệt, Đã duyệt, Từ chối
        public DateTime NgayGuiDon { get; set; } = DateTime.Now;
    }
}