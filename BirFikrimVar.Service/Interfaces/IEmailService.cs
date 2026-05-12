using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BirFikrimVar.Service.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);

        Task SendWelcomeEmailAsync(string userEmail, string userName);
        Task SendPasswordResetEmailAsync(string userEmail, string userName, string token);

    }
}