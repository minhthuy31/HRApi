using System.ComponentModel.DataAnnotations;

namespace HRApi.Models
{
    public class ChamCong
    {
        [Key]
        public int Id { get; set; }
        public string MaNhanVien { get; set; }
        public DateTime NgayChamCong { get; set; }
        public double NgayCong { get; set; }
        public string? GhiChu { get; set; }
    }
}
