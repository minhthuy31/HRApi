using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRApi.Models
{
    public class DonNghiPhep
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string MaNhanVien { get; set; }

        [ForeignKey("MaNhanVien")]
        public NhanVien NhanVien { get; set; }

        [Required]
        public DateTime NgayBatDau { get; set; }

        [Required]
        public DateTime NgayKetThuc { get; set; }

        [Required]
        public double SoNgayNghi { get; set; }

        [Required]
        public string LyDo { get; set; }

        public string? TepDinhKem { get; set; }

        [Required]
        public string TrangThai { get; set; } = "Chờ duyệt";

        public DateTime NgayGuiDon { get; set; } = DateTime.Now;
    }
}