using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace HRApi.Models
{
    public class ChucVuNhanVien
    {
        [Key]
        public string MaChucVuNV { get; set; }

        [Required]
        public string TenChucVu { get; set; }

        public double? HSPC { get; set; } // Hệ số phụ cấp chức vụ (nếu có)

        public string? MoTaCongViec { get; set; }

        // Liên kết với Role hệ thống (VD: Chức vụ "Trưởng phòng" -> Role "Manager")
        public int? RoleId { get; set; }
        [ForeignKey("RoleId")]
        public virtual UserRole? UserRole { get; set; }

        // --- NAVIGATION ---
        [JsonIgnore]
        public virtual ICollection<NhanVien>? NhanViens { get; set; }
    }
}