using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using BirFikrimVar.Service.Interfaces;
using System;
using System.Threading.Tasks;

namespace BirFikrimVar.Service.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");

                var message = new MimeMessage();
                var fromName = emailSettings["FromName"] ?? "BirFikrimVar";
                var fromEmail = emailSettings["FromEmail"];

                message.From.Add(new MailboxAddress(fromName, fromEmail));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder { HtmlBody = isHtml ? body : null, TextBody = !isHtml ? body : null };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();

                var server = emailSettings["SmtpServer"];
                var port = int.Parse(emailSettings["SmtpPort"] ?? "587");

                await client.ConnectAsync(server, port, SecureSocketOptions.StartTls);

                var user = emailSettings["SmtpUsername"];
                var pass = emailSettings["SmtpPassword"];
                await client.AuthenticateAsync(user, pass);

                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("{ToEmail} adresine e-posta başarıyla gönderildi.", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "E-posta gönderimi sırasında hata!");
            }
        }

        public async Task SendWelcomeEmailAsync(string userEmail, string userName)
        {
            var subject = "BirFikrimVar'a Hoş Geldiniz!";
            var body = $"<h1>Merhaba {userName}</h1><p>Fikirlerini paylaşmaya hazır mısın?</p>";
            await SendEmailAsync(userEmail, subject, body);
        }

        public async Task SendPasswordResetEmailAsync(string userEmail, string userName, string token)
        {
            var subject = "Şifre Sıfırlama Talebi";
            var body = $"<p>Sayın {userName}, şifrenizi sıfırlamak için kodunuz: <b>{token}</b></p>";
            await SendEmailAsync(userEmail, subject, body);
        }
    }
}
