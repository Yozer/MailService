using System.Collections.Generic;

namespace MailService.Api.Dto
{
    public class PatchEmailDto
    {
        public string Subject { get; set; }
        public string Body { get; set; }
        public string Sender { get; set; }
        public List<string> To { get; set; }
    }
}