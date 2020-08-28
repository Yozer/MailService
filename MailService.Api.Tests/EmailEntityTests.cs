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
    }
}
