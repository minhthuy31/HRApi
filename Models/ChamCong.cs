using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRApi.Models
{
    public class ChamCong
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string MaNhanVien { get; set; }

        [ForeignKey("MaNhanVien")]
        public virtual NhanVien NhanVien { get; set; }

        [Required]
        public DateTime NgayChamCong { get; set; } // Sẽ là giờ Check-in

        public DateTime? GioCheckOut { get; set; } // Giờ check-out (có thể null)

        public double NgayCong { get; set; }
        public string? GhiChu { get; set; }
    }
}