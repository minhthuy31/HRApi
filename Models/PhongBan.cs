using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HRApi.Models
{
    public class PhongBan
    {
        [Key]
        public string MaPhongBan { get; set; }

        [Required]
        public string TenPhongBan { get; set; }

        public string? DiaChi { get; set; }
        public string? sdt_PhongBan { get; set; }
        public bool TrangThai { get; set; } = true;

        // --- NAVIGATION ---
        [JsonIgnore]
        public virtual ICollection<NhanVien>? NhanViens { get; set; }

        public PhongBan()
        {
            NhanViens = new HashSet<NhanVien>();
        }
    }
}