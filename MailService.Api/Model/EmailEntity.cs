using System.Collections.Generic;
using System.Linq;
using FluentValidation;

namespace MailService.Api.Model
{
    public class EmailEntity : Entity
    {
        public string Subject { get; private set; }
        public string Body { get; private set; }
        public string Sender { get; private set; }
        public EmailStatus Status { get; private set; }
        public EmailPriority Priority { get; private set; }

        private List<string> _to;
        public IReadOnlyList<string> To => _to?.AsReadOnly();

        private List<EmailAttachmentEntity> _attachments;
        public IReadOnlyList<EmailAttachmentEntity> Attachments => _attachments?.AsReadOnly();

        public EmailEntity(string subject, string body, string sender, IEnumerable<string> to) 
            : this(subject, body, sender, to, EmailPriority.Normal)
        {
        }

        public EmailEntity(string subject, string body, string sender, IEnumerable<string> to, EmailPriority priority)
        {
            Subject = subject;
            Body = body;
            Sender = sender;
            _to = to?.ToList() ?? new List<string>();
            _attachments = new List<EmailAttachmentEntity>();
            Status = EmailStatus.Pending;
            Priority = priority;

            ValidateAndThrow();
        }

        private void ValidateAndThrow()
        {
            var validator = new EmailEntityValidator();
            validator.ValidateAndThrow(this);
        }

        private class EmailEntityValidator : AbstractValidator<EmailEntity>
        {
            public EmailEntityValidator()
            {
                RuleFor(t => t.Subject).NotEmpty();
                RuleFor(t => t.Body).NotEmpty();
                RuleFor(t => t.Sender).NotEmpty().EmailAddress();

                RuleFor(t => t.To)
                    .NotEmpty()
                    .ForEach(email => email.NotEmpty().EmailAddress());

                RuleForEach(t => t.Attachments)
                    .SetValidator(new AttachmentValidator());
            }
        }

        private class AttachmentValidator : AbstractValidator<EmailAttachmentEntity>
        {
            public AttachmentValidator()
            {
                RuleFor(t => t.Name).NotEmpty();
                RuleFor(t => t.Data).NotEmpty();
            }
        }
    }
}
