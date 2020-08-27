using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace MailService.Api.Infrastructure.Smtp
{
    public class SmtpClientWrapper : ISmtpClient
    {
        private readonly SmtpClient _client;

        public SmtpClientWrapper(IOptions<SmtpOptions> options)
        {
            var smtpOptions = options.Value;
            _client = new SmtpClient(smtpOptions.Server, smtpOptions.Port)
            {
                EnableSsl = smtpOptions.UseSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            _client.Credentials = new NetworkCredential(smtpOptions.Login, smtpOptions.Password);
        }

        public void Dispose()
        {
            _client?.Dispose();
        }

        public Task SendMailAsync(MailMessage mailMessage) 
            => _client.SendMailAsync(mailMessage);
    }
}