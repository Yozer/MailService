using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using MailService.Api.Commands;
using MailService.Api.Commands.Handlers;
using MailService.Api.Infrastructure.Smtp;
using MailService.Api.Model;
using MediatR;
using NSubstitute;
using NUnit.Framework;

namespace MailService.Api.Tests.Commands
{
    public class SendPendingEmailsCommandHandlerTests : TestBase
    {
        private IEmailsRepository _repository;
        private ISmtpClient _smtpClient;
        private IRequestHandler<SendPendingEmailsCommand> _handler;

        [SetUp]
        public void SetUp()
        {
            _repository = Freeze<IEmailsRepository>();
            _smtpClient = Freeze<ISmtpClient>();
            _handler = Create<SendPendingEmailsCommandHandler>();
        }

        [Theory, AutoData]
        public async Task SendPendingEmailCommand_ShouldSendEmails(
            SendPendingEmailsCommand command, 
            List<string> fileNames,
            List<byte[]> payloads)
        {
            // arrange
            var emailEntity = Create<EmailEntity>();
            var expectedAttachments = fileNames.Zip(payloads).ToList();
            foreach (var (fileName, payload) in expectedAttachments)
            {
                emailEntity.AddAttachment(fileName, payload);
            }

            _repository.GetEmails(EmailStatus.Pending)
                .Returns(new List<EmailEntity> {emailEntity}.ToAsyncEnumerable());

            // act
            await _handler.Handle(command, CancellationToken.None);

            // assert
            await _smtpClient.Received(1)
                .SendMailAsync(Arg.Is<MailMessage>(mail => mail.BodyEncoding == Encoding.UTF8 &&
                                                           mail.SubjectEncoding == Encoding.UTF8 &&
                                                           mail.From.Address == emailEntity.Sender &&
                                                           mail.To.Select(t => t.Address)
                                                               .SequenceEqual(emailEntity.To) &&
                                                           mail.Subject == emailEntity.Subject &&
                                                           mail.Attachments.Select(t => t.Name).SequenceEqual(fileNames) &&
                                                           mail.Body == emailEntity.Body));
            await _repository.Received(1)
                .UpdateEmail(Arg.Is<EmailEntity>(t => t.Status == EmailStatus.Sent &&
                                                      t.Id == emailEntity.Id));

            emailEntity.Status.Should().Be(EmailStatus.Sent);
        }
    }
}