namespace HRApi.Models
{
    public class ActiveQRToken
    {
        public int Id { get; set; }
        public string Token { get; set; } // Chuỗi bí mật ngẫu nhiên
        public DateTime ExpiresAt { get; set; } // Thời điểm hết hạn 
        public bool IsUsed { get; set; } // Đã bị dùng chưa?
    }
}
