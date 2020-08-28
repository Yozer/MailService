using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.NUnit3;
using FluentAssertions;
using MailService.Api.Commands;
using MailService.Api.Commands.Handlers;
using MailService.Api.Model;
using NSubstitute;
using NUnit.Framework;

namespace MailService.Api.Tests.Commands
{
    public class CreateEmailCommandHandlerTests : TestBase
    {
        private IEmailsRepository _repository;
        private CreateEmailCommandHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _repository = Freeze<IEmailsRepository>();
            _handler = Create<CreateEmailCommandHandler>();
        }

        [Test]
        public async Task ShouldMapDtoToDomainObjectAndInsertIt()
        {
            // arrange
            var command = Create<CreateEmailCommand>();
            
            // act
            var returnedDto = await _handler.Handle(command, CancellationToken.None);

            // assert
            await _repository.Received(1)
                .AddNewEmail(Arg.Is<EmailEntity>(t => t.Status == EmailStatus.Pending &&
                                                      t.Attachments.Count == 0 &&
                                                      t.To.SequenceEqual(command.Email.To) &&
                                                      t.Sender == command.Email.Sender &&
                                                      t.Priority == command.Email.Priority &&
                                                      t.Subject == command.Email.Subject &&
                                                      t.Body == command.Email.Body));

            returnedDto.Should().BeEquivalentTo(command.Email);
        }
    }
}
