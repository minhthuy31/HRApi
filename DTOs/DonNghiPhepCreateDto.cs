using System.ComponentModel.DataAnnotations;

namespace HRApi.DTOs
{
    public class DonNghiPhepCreateDto
    {
        [Required(ErrorMessage = "Mã nhân viên là bắt buộc.")]
        public string MaNhanVien { get; set; }

        [Required(ErrorMessage = "Ngày nghỉ là bắt buộc.")]
        public DateTime NgayNghi { get; set; }

        public string? LyDo { get; set; }
    }
}
