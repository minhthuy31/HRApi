using System.ComponentModel.DataAnnotations;

namespace HRApi.DTOs
{
    public class ChamCongUpsertDto
    {
        [Required]
        public string MaNhanVien { get; set; }
        [Required]
        public string NgayChamCong { get; set; } // Sẽ nhận là "YYYY-MM-DD"
        public double NgayCong { get; set; }
        public string? GhiChu { get; set; }
        public bool OnlyIfEmpty { get; set; } = false;
    }
}
