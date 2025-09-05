namespace Login.Models.DTOs
{
    public class ResetPasswordEmailModel
    {
        public string Email { get; set; } = string.Empty;   
        public string Code { get; set; } = string.Empty;
    }
}
