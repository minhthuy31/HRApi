using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HRApi.Models
{
    public class ChucVuNhanVien
    {
        [Key]
        public string MaChucVuNV { get; set; }
        public string TenChucVu { get; set; }
        public double? HSPC { get; set; }

        [JsonIgnore]
        public virtual ICollection<NhanVien>? NhanViens { get; set; }
    }
}