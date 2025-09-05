using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRApi.Models
{
    public class NhanVien
    {
        [Key]
        public string MaNhanVien { get; set; }
        public string MatKhau { get; set; }
        public string? HoTen { get; set; }
        public DateTime? NgaySinh { get; set; }
        public int? GioiTinh { get; set; }
        public string? DanToc { get; set; }
        public string? TinhTrangHonNhan { get; set; }
        public string? QueQuan { get; set; }
        public string? DiaChiThuongTru { get; set; }
        public string? DiaChiTamTru { get; set; }
        public string? HinhAnh { get; set; }
        public string? sdt_NhanVien { get; set; }
        public string? Email { get; set; }
        public string? CCCD { get; set; }
        public DateTime? NgayCapCCCD { get; set; }
        public string? NoiCapCCCD { get; set; }
        public string? LoaiNhanVien { get; set; }
        public bool TrangThai { get; set; }
        public string? SoTaiKhoanNH { get; set; }
        public string? TenNganHang { get; set; }

        // Foreign keys
        public string? MaChucVuNV { get; set; }
        public string? MaPhongBan { get; set; }
        public string? MaChuyenNganh { get; set; }
        public string? MaTrinhDoHocVan { get; set; }

        // Navigation properties
        [ForeignKey("MaPhongBan")]
        public virtual PhongBan? PhongBan { get; set; }

        [ForeignKey("MaChucVuNV")]
        public virtual ChucVuNhanVien? ChucVuNhanVien { get; set; }

        [ForeignKey("MaChuyenNganh")]
        public virtual ChuyenNganh? ChuyenNganh { get; set; }

        [ForeignKey("MaTrinhDoHocVan")]
        public virtual TrinhDoHocVan? TrinhDoHocVan { get; set; }
    }
}

