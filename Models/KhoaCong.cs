using System.ComponentModel.DataAnnotations;

namespace HRApi.Models
{
    public class KhoaCong
    {
        [Key]
        public int Id { get; set; }
        public int Thang { get; set; }
        public int Nam { get; set; }
        public bool IsLocked { get; set; } = false;
    }
}
