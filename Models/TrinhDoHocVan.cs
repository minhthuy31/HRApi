using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HRApi.Models
{
    public class TrinhDoHocVan
    {
        [Key]
        public string MaTrinhDoHocVan { get; set; }
        public string TenTrinhDo { get; set; }
        public double? HeSoBac { get; set; }

        [JsonIgnore]
        public virtual ICollection<NhanVien>? NhanViens { get; set; }
    }
}
