using System.ComponentModel.DataAnnotations;

namespace HRApi.Models
{
    public class BangLuong
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string MaNhanVien { get; set; }

        [Required]
        public int Thang { get; set; }

        [Required]
        public int Nam { get; set; }

        public decimal LuongCoBan { get; set; }
        public double TongNgayCong { get; set; }
        public decimal LuongThucNhan { get; set; }
        public DateTime NgayTinhLuong { get; set; } = DateTime.UtcNow;
    }
}
