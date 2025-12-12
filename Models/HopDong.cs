using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace HRApi.Models
{
    public class HopDong
    {
        [Key]
        public string MaHopDong { get; set; } // Số hợp đồng

        // Loại hợp đồng (VD: Xác định thời hạn, Thử việc...)
        public string? LoaiHopDong { get; set; }

        // Thời hạn hợp đồng (VD: 12 tháng, 24 tháng...)
        public int? ThoiHanHopDong { get; set; }

        [Required]
        public DateTime NgayKy { get; set; } // Ngày ký

        [Required]
        public DateTime NgayHieuLuc { get; set; } // Từ ngày...

        public DateTime? NgayHetHan { get; set; } // ...Đến ngày (null nếu vô thời hạn)

        // --- LƯƠNG & TRỢ CẤP ---
        [Column(TypeName = "decimal(18,2)")]
        public decimal LuongCoBan { get; set; } = 0; // Lương chính

        [Column(TypeName = "decimal(18,2)")]
        public decimal LuongDongBaoHiem { get; set; } = 0; // Lương đóng BHXH

        [Column(TypeName = "decimal(18,2)")]
        public decimal PhuCapTrachNhiem { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal PhuCapAnTrua { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal PhuCapKhac { get; set; } = 0;

        public string? GhiChu { get; set; }

        // Trạng thái: true = Hợp đồng đang hiệu lực, false = Hết hạn/Đã thanh lý
        public bool TrangThai { get; set; } = true;

        // --- KHÓA NGOẠI ---
        [ForeignKey("NhanVien")]
        public string MaNhanVien { get; set; }

        [JsonIgnore]
        public virtual NhanVien? NhanVien { get; set; }
    }
}