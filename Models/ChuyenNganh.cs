using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HRApi.Models
{
    public class ChuyenNganh
    {
        [Key]
        public string MaChuyenNganh { get; set; }

        public string? TenChuyenNganh { get; set; }

        // Navigation property: Một chuyên ngành có thể có nhiều nhân viên
        [JsonIgnore]
        public virtual ICollection<NhanVien>? NhanViens { get; set; }
    }
}
