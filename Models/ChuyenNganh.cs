using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HRApi.Models
{
    public class ChuyenNganh
    {
        [Key]
        public string MaChuyenNganh { get; set; }

        public string? TenChuyenNganh { get; set; }

        // có nhiều nhân viên trong một chuyên ngành
        [JsonIgnore]
        public virtual ICollection<NhanVien>? NhanViens { get; set; }
    }
}
