using System.Collections.Generic;
using System.Linq;
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
    public class GetAllEmailsQueryHandlerTests : TestBase
    {
        private IEmailsRepository _repository;
        private GetAllEmailsQueryHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _repository = Freeze<IEmailsRepository>();
            _handler = Create<GetAllEmailsQueryHandler>();
        }

        [Theory, AutoData]
        public async Task ShouldFetchEmailFromRepoAndMapIt(GetAllEmailsQuery query)
        {
            // arrange
            var expectedEmails = Create<List<EmailEntity>>();
            _repository.GetAllEmails()
                .Returns(expectedEmails.ToAsyncEnumerable());

            // act
            var actualEmails = await _handler.Handle(query, CancellationToken.None);

            // assert
            actualEmails.Should().BeEquivalentTo(expectedEmails);
        }
    }
}