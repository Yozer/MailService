using System;
using System.Net.Mail;
using System.Threading.Tasks;

namespace MailService.Api.Infrastructure.Smtp
{
    public interface ISmtpClient : IDisposable
    {
        Task SendMailAsync(MailMessage mailMessage);
    }
}
