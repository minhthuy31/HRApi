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
        public NhanVien? NhanVien { get; set; }

        public DateTime NgayNghi { get; set; }

        // THÊM THUỘC TÍNH NÀY VÀO
        public DateTime NgayGuiDon { get; set; }

        public string? LyDo { get; set; }

        public string TrangThai { get; set; }
    }
}