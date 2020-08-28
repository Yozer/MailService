using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using MailService.Api.Model;
using MailService.Api.Queries;
using MailService.Api.Queries.Handlers;
using NSubstitute;
using NUnit.Framework;

namespace MailService.Api.Tests.Queries
{
    public class GetEmailQueryHandlerTests : TestBase
    {
        private IEmailsRepository _repository;
        private GetEmailQueryHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _repository = Freeze<IEmailsRepository>();
            _handler = Create<GetEmailQueryHandler>();
        }

        [Theory, AutoData]
        public async Task ShouldFetchEmailFromRepoAndMapIt(GetEmailQuery query)
        {
            // arrange
            var expectedEmail = Create<EmailEntity>();
            _repository.GetEmail(query.Id)
                .Returns(expectedEmail);

            // act
            var actualEmail = await _handler.Handle(query, CancellationToken.None);

            // assert
            actualEmail.Should().BeEquivalentTo(expectedEmail);
        }
    }
}
