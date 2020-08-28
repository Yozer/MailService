using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture.NUnit3;
using FluentAssertions;
using FluentValidation;
using MailService.Api.Model;
using NUnit.Framework;

namespace MailService.Api.Tests
{
    public class EmailEntityTests : TestBase
    {
        private static readonly string ValidEmail = "test@test.com";
        private static readonly IEnumerable<string> ValidRecipients = new[] {ValidEmail};

        [TestCase(null)]
        [TestCase(" ")]
        [TestCase("")]
        public void ShouldThrow_WhenSubjectIsInvalid(string subject)
        {
            Action act = () => new EmailEntity(subject, Create<string>(), ValidEmail, ValidRecipients);
            act.Should().ThrowExactly<ValidationException>();
        }

        [TestCase(null)]
        [TestCase(" ")]
        [TestCase("")]
        public void ShouldThrow_WhenBodyIsInvalid(string body)
        {
            Action act = () => new EmailEntity(Create<string>(), body, ValidEmail, ValidRecipients);
            act.Should().ThrowExactly<ValidationException>();
        }

        [TestCase(null)]
        [TestCase(" ")]
        [TestCase("")]
        [TestCase("qwe")]
        public void ShouldThrow_WhenSenderIsInvalid(string sender)
        {
            Action act = () => new EmailEntity(Create<string>(), Create<string>(), sender, ValidRecipients);
            act.Should().ThrowExactly<ValidationException>();
        }

        [Test()]
        public void ShouldThrow_WhenOneRecipientIsInvalid()
        {
            // arrange
            var recipients = ValidRecipients.ToList();
            recipients.Add(Create<string>());

            // act
            Action act = () => new EmailEntity(Create<string>(), Create<string>(), ValidEmail, recipients);

            // assert
            act.Should().ThrowExactly<ValidationException>();
        }

        [Test]
        public void ShouldThrow_WhenRecipientsAreEmpty()
        {
            // arrange
            // ReSharper disable once CollectionNeverUpdated.Local
            var recipients = new List<string>();

            // act
            Action act = () => new EmailEntity(Create<string>(), Create<string>(), ValidEmail, recipients);

            // assert
            act.Should().ThrowExactly<ValidationException>();
        }

        [Test]
        public void ShouldThrow_WhenRecipientsAreNull()
        {
            // act
            Action act = () => new EmailEntity(Create<string>(), Create<string>(), ValidEmail, null);

            // assert
            act.Should().ThrowExactly<ValidationException>();
        }


        [Theory, AutoData]
        public void ShouldSuccessfullyCreateEmail_WhenValidObjectIsGiven(string subject, string body, EmailPriority priority)
        {
            // act
            var email = new EmailEntity(subject, body, ValidEmail, ValidRecipients, priority);

            // assert
            email.Subject.Should().Be(subject);
            email.Sender.Should().Be(ValidEmail);
            email.Priority.Should().Be(priority);
            email.Body.Should().Be(body);
            email.To.Should().BeEquivalentTo(ValidRecipients);
        }

        [Theory, AutoData]
        public void ShouldAddAttachment_ForSmallAttachment(string attachment, byte[] data)
        {
            // arrange
            var email = Create<EmailEntity>();

            // act
            email.AddAttachment(attachment, data);

            // assert
            email.Attachments.Should().HaveCount(1);
            email.Attachments[0].Data.Should().BeEquivalentTo(data);
            email.Attachments[0].Name.Should().Be(attachment);
        }

        [Theory, AutoData]
        public void ShouldThrow_WhenAddingAttachment_ForLargeAttachment(string attachment)
        {
            // arrange
            var email = Create<EmailEntity>();
            var data = Enumerable.Repeat((byte)0, 10 * 1024 * 1024 + 1).ToArray();

            // act
            Action act = () => email.AddAttachment(attachment, data);

            // assert
            act.Should().ThrowExactly<ValidationException>()
                .WithMessage("*Attachment size should be smaller than 10485760 bytes");
        }

        [Test]
        public void ShouldThrow_WhenAddingAttachment_ForEmailAlreadySent()
        {
            // arrange
            var email = Create<EmailEntity>();
            email.ChangeStatusToSent();

            // act
            Action act = () => email.AddAttachment(default, default);

            // assert
            act.Should().ThrowExactly<ValidationException>()
                .WithMessage("Unable to update email. It was already sent.");
        }

        [Test]
        public void UpdateSender_ShouldFailWhenEmailWasSent()
        {
            // arrange
            var email = Create<EmailEntity>();
            email.ChangeStatusToSent();

            // act
            Action act = () => email.UpdateSender(ValidEmail);

            // assert
            act.Should().ThrowExactly<ValidationException>()
                .WithMessage("Unable to update email. It was already sent.");
        }

        [Test]
        public void UpdateSender_ShouldFailForInvalidEmail()
        {
            // arrange
            var email = Create<EmailEntity>();

            // act
            Action act = () => email.UpdateSender(Create<string>());

            // assert
            act.Should().ThrowExactly<ValidationException>()
                .WithMessage("*'Sender' is not a valid email address.");
        }

        [Test]
        public void UpdateSender_ShouldSuccessForValidEmail()
        {
            // arrange
            var email = Create<EmailEntity>();

            // act
            Action act = () => email.UpdateSender(ValidEmail);

            // assert
            act.Should().NotThrow();
            email.Sender.Should().Be(ValidEmail);
        }
    }
}
