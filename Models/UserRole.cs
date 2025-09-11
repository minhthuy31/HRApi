using System.ComponentModel.DataAnnotations;

namespace HRApi.Models
{
    public class UserRole
    {
        [Key]
        public int RoleId { get; set; }
        [Required]
        public string NameRole { get; set; }
        // 1 role sẽ có nhiều nhân viên
        public virtual ICollection<NhanVien> NhanViens { get; set; }

        public UserRole()
        {
            NhanViens = new HashSet<NhanVien>();
        }
    }
}
