using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FluentValidation;
using FluentValidation.Results;

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

        public IReadOnlyList<string> To
        {
            get => _to?.AsReadOnly();
            set => _to = value.ToList();
        }

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

        public void ChangeStatusToSent()
        {
            if(Status == EmailStatus.Sent)
                throw new ValidationException("Email was already sent");

            Status = EmailStatus.Sent;
        }

        public void AddAttachment(string fileName, byte[] data)
        {
            ThrowIfAlreadySent();

            _attachments.Add(new EmailAttachmentEntity(fileName, data));
            var validated = Validate();
            if (!validated.IsValid)
            {
                _attachments.RemoveAt(_attachments.Count - 1); // O(1)
                throw new ValidationException(validated.Errors);
            }
        }

        public void UpdateSender(string sender) 
            => TryUpdate(t => t.Sender, sender);

        public void UpdateTo(List<string> to)
            => TryUpdate(t => t.To, to);

        public void UpdateBody(string body)
            => TryUpdate(t => t.Body, body);

        public void UpdateSubject(string subject)
            => TryUpdate(t => t.Subject, subject);

        public void UpdatePriority(EmailPriority priority)
            => TryUpdate(t => t.Priority, priority);

        private void TryUpdate<T>(Expression<Func<EmailEntity, T>> expression, T newValue)
        {
            ThrowIfAlreadySent();

            var func = expression.Compile();
            var oldValue = func(this);
            var memberExpression = (MemberExpression)expression.Body;
            var propertyInfo = (PropertyInfo)memberExpression.Member;

            propertyInfo.SetValue(this, newValue);
            var validated = Validate();

            if (!validated.IsValid)
            {
                propertyInfo.SetValue(this, oldValue);
                throw new ValidationException(validated.Errors);
            }
        }


        private void ValidateAndThrow()
        {
            var validator = new EmailEntityValidator();
            validator.ValidateAndThrow(this);
        }

        private void ThrowIfAlreadySent()
        {
            if(Status == EmailStatus.Sent)
                throw new ValidationException("Unable to update email. It was already sent.");
        }

        private ValidationResult Validate()
        {
            var validator = new EmailEntityValidator();
            return validator.Validate(this);
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
            private const int MaxAttachmentSizeInBytes = 10 * 1024 * 1024; //10MB

            public AttachmentValidator()
            {
                RuleFor(t => t.Name).NotEmpty();
                RuleFor(t => t.Data).NotEmpty();
                RuleFor(t => t.Data.Length)
                    .LessThanOrEqualTo(MaxAttachmentSizeInBytes)
                    .WithMessage($"Attachment size should be smaller than {MaxAttachmentSizeInBytes} bytes");
            }
        }
    }
}
