using System.ComponentModel.DataAnnotations;

namespace HRApi.Models
{
    public class ChamCong
    {
        [Key]
        public int Id { get; set; }
        public string MaNhanVien { get; set; }
        public DateTime NgayChamCong { get; set; }
        public string TrangThai { get; set; }
        public TimeSpan? GioVao { get; set; }
        public TimeSpan? GioRa { get; set; }
        public string? GhiChu { get; set; }
    }
}
