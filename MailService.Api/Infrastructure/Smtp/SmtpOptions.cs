using System.Security;

namespace MailService.Api.Infrastructure.Smtp
{
    public class SmtpOptions
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public bool UseSsl { get; set; }
    }
}
