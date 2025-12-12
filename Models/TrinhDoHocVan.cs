using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HRApi.Models
{
    public class TrinhDoHocVan
    {
        [Key]
        public string MaTrinhDoHocVan { get; set; }

        [Required]
        public string TenTrinhDo { get; set; } // VD: Đại học, Cao đẳng...

        public double? HeSoBac { get; set; } // Hệ số lương theo bằng cấp (nếu áp dụng)

        public string? MoTa { get; set; }

        // --- NAVIGATION ---
        [JsonIgnore]
        public virtual ICollection<NhanVien>? NhanViens { get; set; }
    }
}