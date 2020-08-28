using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MailService.Api.Commands;
using MailService.Api.Commands.Handlers;
using MailService.Api.Dto;
using MailService.Api.Model;
using NSubstitute;
using NUnit.Framework;

namespace MailService.Api.Tests.Commands
{
    public class PatchEmailCommandHandlerTests : TestBase
    {
        private IEmailsRepository _repository;
        private PatchEmailCommandHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _repository = Freeze<IEmailsRepository>();
            _handler = Create<PatchEmailCommandHandler>();
        }

        [Test]
        public async Task ShouldPatchTheEmail()
        {
            // arrange
            var command = Create<PatchEmailCommand>();
            var emailEntity = Create<EmailEntity>();
            _repository.GetEmail(command.Id)
                .Returns(emailEntity);

            // act
            var foundEmailToUpdate = await _handler.Handle(command, CancellationToken.None);

            // assert
            await _repository.Received(1)
                .UpdateEmail(Arg.Is<EmailEntity>(t => t.Status == EmailStatus.Pending &&
                                                      t.Attachments.Count == 0 &&
                                                      t.Id == emailEntity.Id &&
                                                      t.To.SequenceEqual(command.Patch.To) &&
                                                      t.Sender == command.Patch.Sender &&
                                                      t.Priority == command.Patch.Priority &&
                                                      t.Subject == command.Patch.Subject &&
                                                      t.Body == command.Patch.Body));

            foundEmailToUpdate.Should().BeTrue();
        }

        [Test]
        public async Task ShouldNotUpdateEmail_WhenNothingWasChanged()
        {
            // arrange
            var emailEntity = Create<EmailEntity>();
            _repository.GetEmail(emailEntity.Id)
                .Returns(emailEntity);

            var patch = new CreateEmailDto
            {
                Priority = emailEntity.Priority,
                To = emailEntity.To.ToList(),
                Sender = emailEntity.Sender,
                Body = emailEntity.Body,
                Subject = emailEntity.Subject
            };
            var command = new PatchEmailCommand(emailEntity.Id, patch);

            // act
            var foundEmailToUpdate = await _handler.Handle(command, CancellationToken.None);

            // assert
            await _repository.DidNotReceiveWithAnyArgs()
                .UpdateEmail(default);

            foundEmailToUpdate.Should().BeTrue();
        }

        [Test]
        public async Task ShouldIgnoreIfEmailDoestExist()
        {
            var command = Create<PatchEmailCommand>();
            _repository.GetEmail(command.Id)
                .Returns((EmailEntity)null);
            
            // act
            var foundEmailToUpdate = await _handler.Handle(command, CancellationToken.None);

            // assert
            foundEmailToUpdate.Should().BeFalse();
            await _repository.DidNotReceiveWithAnyArgs().UpdateEmail(default);
        }
    }
}