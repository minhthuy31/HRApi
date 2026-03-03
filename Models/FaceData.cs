using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRApi.Models
{
    public class FaceData
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string MaNhanVien { get; set; } // Foreign Key to NhanVien

        [Required]
        public string FaceDescriptor { get; set; } // Stored as JSON string (float array)

        // Navigation property (optional, good for EF Core)
        [ForeignKey("MaNhanVien")]
        public virtual NhanVien NhanVien { get; set; }
    }
}