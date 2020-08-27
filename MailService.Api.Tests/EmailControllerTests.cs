using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using AutoFixture;
using FluentAssertions;
using MailService.Api.Commands;
using MailService.Api.Controllers;
using MailService.Api.Dto;
using MailService.Api.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace MailService.Api.Tests
{
    public class EmailControllerTests : TestBase
    {
        private EmailsController _controller;
        private IMediator _mediator;

        [SetUp]
        public void SetUp()
        {
            _mediator = Freeze<IMediator>();
            _controller = Fixture.Build<EmailsController>().OmitAutoProperties().Create();
        }

        [Theory, AutoData]
        public async Task ShouldReachToMediator_ForAllEmails(List<EmailDto> expectedEmails)
        {
            // arrange
            _mediator.Send(Arg.Any<GetAllEmailsQuery>())
                .Returns(expectedEmails);

            // act
            var actualEmails = await _controller.GetAll();

            // assert
            actualEmails.Value.Should().BeEquivalentTo(expectedEmails);
        }

        [Theory, AutoData]
        public async Task ShouldReturnNotFound_WhenEmailDoesntExist(Guid id)
        {
            // arrange
            _mediator.Send(Arg.Is<GetEmailQuery>(t => t.Id == id))
                .Returns((EmailDto) null);

            // act
            var actualEmail = await _controller.GetOne(id);

            // assert
            actualEmail.Result.Should().BeOfType<NotFoundResult>();
        }

        [Theory, AutoData]
        public async Task ShouldReturnEmail_WhenRequestedById(EmailDto expectedEmail)
        {
            // arrange
            _mediator.Send(Arg.Is<GetEmailQuery>(t => t.Id == expectedEmail.Id))
                .Returns(expectedEmail);

            // act
            var actualEmail = await _controller.GetOne(expectedEmail.Id);

            // assert
            actualEmail.Value.Should().BeEquivalentTo(expectedEmail);
        }

        [Theory, AutoData]
        public async Task ShouldReturnNotFound_ForStatusRequest_WhenEmailDoesntExist(Guid id)
        {
            // arrange
            _mediator.Send(Arg.Is<GetEmailQuery>(t => t.Id == id))
                .Returns((EmailDto) null);

            // act
            var actualStatus = await _controller.GetOneStatus(id);

            // assert
            actualStatus.Result.Should().BeOfType<NotFoundResult>();
        }

        [Theory, AutoData]
        public async Task ShouldReturnStatus_WhenRequestedById(EmailDto expectedEmail)
        {
            // arrange
            _mediator.Send(Arg.Is<GetEmailQuery>(t => t.Id == expectedEmail.Id))
                .Returns(expectedEmail);

            // act
            var actualStatus = await _controller.GetOneStatus(expectedEmail.Id);

            // assert
            actualStatus.Value.Should()
                .BeEquivalentTo(new EmailStatusDto{Status = expectedEmail.Status});
        }

        [Theory, AutoData]
        public async Task ShouldReturnNotFound_ForPatchRequest_WhenEmailDoesntExist(PatchEmailCommand command)
        {
            // arrange
            _mediator.Send(Arg.Is<PatchEmailCommand>(
                    t => t.Id == command.Id &&
                        t.Patch == command.Patch))
                .Returns(false);

            // act
            var actionResult = await _controller.PatchEmail(command.Id, command.Patch);

            // assert
            actionResult.Should().BeOfType<NotFoundResult>();
        }

        [Theory, AutoData]
        public async Task ShouldReturnSuccess_WhenPatchWasRequested(PatchEmailCommand command)
        {
            // arrange
            _mediator.Send(Arg.Is<PatchEmailCommand>(
                    t => t.Id == command.Id &&
                         t.Patch == command.Patch))
                .Returns(true);

            // act
            var actionResult = await _controller.PatchEmail(command.Id, command.Patch);

            // assert
            actionResult.Should().BeOfType<NoContentResult>();
        }

        [Theory, AutoData]
        public async Task ShouldReachToMediator_ToAddAttachments(string id)
        {
            // arrange
            var files = Fixture.CreateMany<IFormFile>().ToList();
            _mediator.Send(Arg.Is<AddAttachmentsToEmailCommand>(
                    t => t.Id == id &&
                         t.Attachments.SequenceEqual(files)))
                .Returns(Unit.Value);

            // act
            var actionResult = await _controller.AddAttachments(id, files);

            // assert
            actionResult.Should().BeOfType<OkResult>();
        }

        [Theory, AutoData]
        public async Task ShouldReachToMediator_ToCreateNewEmail(EmailDto expectedEmail)
        {
            // arrange
            _mediator.Send(Arg.Is<CreateEmailCommand>(
                    t => t.Email == expectedEmail))
                .Returns(expectedEmail);

            // act
            var actionResult = await _controller.AddEmail(expectedEmail);

            // assert
            actionResult.Result.Should().BeOfType<CreatedAtActionResult>();
            ((CreatedAtActionResult) actionResult.Result).Value
                .Should().BeEquivalentTo(expectedEmail);
        }

        
        [Test]
        public async Task ShouldReachToMediator_ToSendPendingEmails()
        {
            // act
            var actionResult = await _controller.SendPendingEmails();

            // assert
            actionResult.Should().BeOfType<OkResult>();
            await _mediator.Received(1)
                .Send(Arg.Any<SendPendingEmailsCommand>());
        }
    }
}
