using System.ComponentModel.DataAnnotations;

namespace HRApi.DTOs
{
    public class DonNghiPhepCreateDto
    {
        [Required]
        public DateTime NgayBatDau { get; set; }
        [Required]
        public DateTime NgayKetThuc { get; set; }
        [Required]
        public double SoNgayNghi { get; set; }
        [Required]
        public string LyDo { get; set; }
        public IFormFile? File { get; set; }
    }
}
