using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DnsClient.Internal;
using MailService.Api.Infrastructure.Smtp;
using MailService.Api.Model;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MailService.Api.Commands.Handlers
{
    public class SendPendingEmailsCommandHandler : AsyncRequestHandler<SendPendingEmailsCommand>
    {
        private readonly IEmailsRepository _repository;
        private readonly ISmtpClient _smtpClient;
        private readonly ILogger<SendPendingEmailsCommandHandler> _logger;

        public SendPendingEmailsCommandHandler(IEmailsRepository repository, ISmtpClient smtpClient, ILogger<SendPendingEmailsCommandHandler> logger)
        {
            _repository = repository;
            _smtpClient = smtpClient;
            _logger = logger;
        }

        protected override async Task Handle(SendPendingEmailsCommand request, CancellationToken cancellationToken)
        {
            await foreach (var email in _repository.GetEmails(EmailStatus.Pending).WithCancellation(cancellationToken))
            {
                _logger.LogInformation($"Preparing mail message for email {email.Id} with {email.Attachments.Count} attachments");

                var streams = email
                    .Attachments
                    .Select(t => new MemoryStream(t.Data))
                    .ToList(); // remember them otherwise will be destroyed too early
                try
                {
                    var mailMessage = BuildMail(email, streams);
                    _logger.LogInformation($"Prepared mail message for email {email.Id}");

                    await _smtpClient.SendMailAsync(mailMessage);
                    _logger.LogInformation($"Sent mail message for email {email.Id}");

                    email.ChangeStatusToSent();
                    await _repository.UpdateEmail(email);
                    _logger.LogInformation($"Successfully changed status to Sent for email {email.Id}");
                }
                finally
                {
                    foreach (var memoryStream in streams)
                    {
                        await memoryStream.DisposeAsync();
                    }
                }
            }
        }

        private static MailMessage BuildMail(EmailEntity email, List<MemoryStream> streams)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(email.Sender),
                Body = email.Body,
                Subject = email.Subject,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8,
                Priority = GetPriority(email.Priority)
            };

            foreach (var to in email.To)
            {
                mailMessage.To.Add(to);
            }

            foreach (var (attachment, stream) in email.Attachments.Zip(streams))
            {
                mailMessage.Attachments.Add(new Attachment(stream, attachment.Name));
            }

            return mailMessage;
        }

        private static MailPriority GetPriority(EmailPriority emailPriority)
        {
            switch (emailPriority)
            {
                case EmailPriority.Low:
                    return MailPriority.Low;
                case EmailPriority.Normal:
                    return MailPriority.Normal;
                case EmailPriority.High:
                    return MailPriority.High;
                default:
                    throw new ArgumentOutOfRangeException(nameof(emailPriority), emailPriority, null);
            }
        }
    }
}
