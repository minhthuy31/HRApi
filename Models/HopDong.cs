using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace HRApi.Models
{
    public class HopDong
    {
        [Key]
        public string MaHopDong { get; set; }

        // Foreign Key
        [ForeignKey("NhanVien")]
        public string MaNhanVien { get; set; }
        public string? LoaiHopDong { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public string? GhiChu { get; set; }

        // Navigation property
        [JsonIgnore]
        public virtual NhanVien? NhanVien { get; set; }
    }
}
