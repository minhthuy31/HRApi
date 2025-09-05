namespace Login.Services
{
    public interface IEmailService
    {
        Task SendResetPasswordEmail(string to, string code);
        Task SendEmailAsync(string to, string subject, string body);
    }
}
