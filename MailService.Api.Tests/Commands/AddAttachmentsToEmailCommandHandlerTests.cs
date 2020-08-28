using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.NUnit3;
using FluentAssertions;
using MailService.Api.Commands;
using MailService.Api.Commands.Handlers;
using MailService.Api.Model;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using NUnit.Framework;

namespace MailService.Api.Tests.Commands
{
    public class AddAttachmentsToEmailCommandHandlerTests : TestBase
    {
        private IEmailsRepository _repository;
        private AddAttachmentsToEmailCommandHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _repository = Freeze<IEmailsRepository>();
            _handler = Create<AddAttachmentsToEmailCommandHandler>();
        }

        [Theory, AutoData]
        public async Task ShouldAddAttachmentsToEntityAndUpdateRepository(string payload, string fileName)
        {
            // arrange
            var attachment = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes(payload)), 0, payload.Length, fileName, fileName);
            var emailEntity = Fixture.Create<EmailEntity>();
            var command = new AddAttachmentsToEmailCommand(emailEntity.Id, new List<IFormFile>{attachment});
            var attachmentsNames = command.Attachments.Select(t => t.FileName);
            _repository.GetEmail(emailEntity.Id)
                .Returns(emailEntity);
            
            // act
            var foundEmailToUpdate = await _handler.Handle(command, CancellationToken.None);

            // assert
            foundEmailToUpdate.Should().BeTrue();
            await _repository.Received(1)
                .UpdateEmail(Arg.Is<EmailEntity>(t =>
                    t.Attachments.Select(x => x.Name).SequenceEqual(attachmentsNames) &&
                    t.Attachments.All(x => x.Data.Length == payload.Length) &&
                    t.Id == emailEntity.Id));

            emailEntity.Attachments.Select(t => t.Name).Should().BeEquivalentTo(attachmentsNames);
            emailEntity.Attachments.Should().OnlyContain(x => x.Data.Length == payload.Length);
        }

        [Test]
        public async Task ShouldIgnoreIfEmailDoestExist()
        {
            var command = Fixture.Create<AddAttachmentsToEmailCommand>();
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