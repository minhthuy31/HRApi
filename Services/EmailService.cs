using Login.Models.DTOs;
using Microsoft.Extensions.Configuration;
using RazorLight;
using System.Net;
using System.Net.Mail;

namespace Login.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly RazorLightEngine _engine;

        public EmailService(IConfiguration config)
        {
            _config = config;
            _engine = new RazorLightEngineBuilder()
                .UseFileSystemProject(Path.Combine(Directory.GetCurrentDirectory(), "EmailTemplates"))
                .UseMemoryCachingProvider()
                .Build();
        }

        public async Task SendResetPasswordEmail(string to, string code)
        {
            var model = new ResetPasswordEmailModel
            {
                Email = to,
                Code = code
            };

            string body = await _engine.CompileRenderAsync("ResetPassword.cshtml", model);
            await SendEmailAsync(to, "Khôi phục mật khẩu", body);
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var smtp = new SmtpClient(_config["Smtp:Host"])
            {
                Port = int.Parse(_config["Smtp:Port"]),
                Credentials = new NetworkCredential(_config["Smtp:User"], _config["Smtp:Pass"]),
                EnableSsl = true
            };

            var mail = new MailMessage
            {
                From = new MailAddress(_config["Smtp:User"]),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mail.To.Add(to);

            await smtp.SendMailAsync(mail);
        }
    }
}
