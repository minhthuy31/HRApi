using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRApi.Models
{
    public class DangKyOT
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string MaNhanVien { get; set; }
        [ForeignKey("MaNhanVien")]
        public NhanVien? NhanVien { get; set; }

        [Required]
        public DateTime NgayLamThem { get; set; }

        [Required]
        public TimeSpan GioBatDau { get; set; }

        [Required]
        public TimeSpan GioKetThuc { get; set; }

        public double SoGio { get; set; } // Tự tính: (End - Start).TotalHours

        public string? LyDo { get; set; }

        public string TrangThai { get; set; } = "Chờ duyệt"; // Chờ duyệt, Đã duyệt, Từ chối
        public DateTime NgayGuiDon { get; set; } = DateTime.Now;
    }
}