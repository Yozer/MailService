using System;
using System.Collections.Generic;
using MailService.Api.Model;

namespace MailService.Api.Dto
{
    public class EmailDto
    {
        public Guid Id { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string Sender { get; set; }
        public EmailStatus Status { get; set; }
        public EmailPriority Priority { get; set; } = EmailPriority.Normal;
        public List<string> To { get; set; }
        public List<string> Attachments { get; set; }
    }
}
