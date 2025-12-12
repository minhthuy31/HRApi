using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HRApi.Models
{
    public class UserRole
    {
        [Key]
        public int RoleId { get; set; }

        [Required]
        public string NameRole { get; set; } // Admin, NhanVien, TruongPhong...

        // --- NAVIGATION ---
        // 1 Role có nhiều Nhân viên
        [JsonIgnore]
        public virtual ICollection<NhanVien> NhanViens { get; set; }

        // 1 Role có thể gán mặc định cho nhiều Chức vụ
        [JsonIgnore]
        public virtual ICollection<ChucVuNhanVien> ChucVuNhanViens { get; set; }

        public UserRole()
        {
            NhanViens = new HashSet<NhanVien>();
            ChucVuNhanViens = new HashSet<ChucVuNhanVien>();
        }
    }
}