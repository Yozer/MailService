using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.NUnit3;
using FluentAssertions;
using MailService.Api.Dto;
using MailService.Api.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using NSubstitute;
using NUnit.Framework;

namespace MailService.Api.Tests.Integration
{
    [Category("Integration")]
    public class EmailEndpointIntegrationTests : TestBase
    {
        private static readonly JsonSerializerOptions JsonSettings = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = {new JsonStringEnumConverter()}
        };
        private CustomWebApplicationFactory _factory;
        private HttpClient _client;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();
        }

        [SetUp]
        public void SetUp()
        {
            var collection = _factory.Services.GetService<IMongoCollection<EmailEntity>>();
            collection.DeleteMany(t => true);
        }

        [Test]
        public async Task AddSeveralEmails_AndTryToReadAllOfThem()
        {
            // arrange
            var expectedEmails = Fixture.Create<List<CreateEmailDto>>();

            // act
            var postResponses = await expectedEmails
                .Select(email => PostAsync("/emails", email))
                .YieldAsync()
                .ToListAsync();

            var getResponse = await GetAsync<List<EmailDto>>("/emails");

            // assert
            postResponses.Should().OnlyContain(t => t.IsSuccessStatusCode);

            var actualEmails = await postResponses.Select(Deserialize<EmailDto>)
                .YieldAsync()
                .ToListAsync();

            actualEmails.Should()
                .BeEquivalentTo(expectedEmails);
            getResponse.Should().BeEquivalentTo(actualEmails);
        }

        [Test]
        public async Task AddOneEmail_AndTryToReadOnlyIt()
        {
            // arrange
            var expectedEmail = Fixture.Create<CreateEmailDto>();

            // act
            var postResponse = await PostAsync("/emails", expectedEmail);
            var insertedEmail = await Deserialize<EmailDto>(postResponse);
            var getResponse = await GetAsync<EmailDto>($"/emails/{insertedEmail.Id}" );

            // assert
            insertedEmail.Should().BeEquivalentTo(expectedEmail);
            getResponse.Should().BeEquivalentTo(insertedEmail);
        }

        [Test]
        public async Task AddOneEmail_AndTryToReadStatus()
        {
            // arrange
            var expectedEmail = Fixture.Create<CreateEmailDto>();

            // act
            var postResponse = await PostAsync("/emails", expectedEmail);
            var insertedEmail = await Deserialize<EmailDto>(postResponse);
            var getResponse = await GetAsync<EmailStatusDto>($"/emails/{insertedEmail.Id}/status" );

            // assert
            getResponse.Status.Should().Be(EmailStatus.Pending);
        }

        [Test]
        public async Task AddOneEmail_AndSendIt_ShouldSendItOnlyOnce()
        {
            // arrange
            var expectedEmail = Fixture.Create<CreateEmailDto>();

            // act
            await PostAsync("/emails", expectedEmail);
            await PostAsync("emails/send-pending", string.Empty);
            await PostAsync("emails/send-pending", string.Empty); // second call shouldnt send anything
            var actualEmails = await GetAsync<List<EmailDto>>("/emails" );

            // assert
            actualEmails.Should().HaveCount(1);
            actualEmails[0].Status.Should().Be(EmailStatus.Sent);
            await _factory.SmtpClient.Received(1)
                .SendMailAsync(Arg.Is<MailMessage>(
                    mail => mail.To.Select(t => t.Address).SequenceEqual(expectedEmail.To) &&
                            mail.From.Address == expectedEmail.Sender &&
                            Equals(mail.BodyEncoding, Encoding.UTF8) &&
                            Equals(mail.SubjectEncoding, Encoding.UTF8) &&
                            mail.Body == expectedEmail.Body &&
                            mail.Attachments.Count == 0 &&
                            mail.Subject == expectedEmail.Subject));
        }

        [Test]
        public async Task PatchInsertedEmail_ShouldChangeItSuccessfully()
        {
            // arrange
            var patchDto = Fixture.Create<CreateEmailDto>();
            await Fixture
                .Create<List<CreateEmailDto>>()
                .Select(email => PostAsync("/emails", email))
                .YieldAsync()
                .ToListAsync();
            var expectedEmails = await GetAsync<List<EmailDto>>("/emails");
            var emailToPatch = expectedEmails[0];

            // act
            await PatchAsync($"emails/{emailToPatch.Id}", patchDto);
            var actualEmails = await GetAsync<List<EmailDto>>("/emails" );

            // assert
            actualEmails.Should().HaveSameCount(expectedEmails);
            actualEmails
                .Where(t => t.Id != emailToPatch.Id)
                .Should()
                .BeEquivalentTo(expectedEmails.Where(t => t.Id != emailToPatch.Id),
                    "All not patched emails should stay the same");

            var actualPatchedEmail = actualEmails.Single(t => t.Id == emailToPatch.Id);
            actualPatchedEmail.Should().BeEquivalentTo(patchDto);
        }

        [Theory, AutoData]
        public async Task UploadSeveralAttachmentsToEmail(List<string> fileNames, List<byte[]> payloads)
        {
            // arrange
            var expectedAttachments = new List<FormFile>();
            foreach (var (file, payload) in fileNames.Zip(payloads))
            {
                expectedAttachments.Add(
                    new FormFile(new MemoryStream(payload), 0, payload.Length, file, file));
            }
            var expectedEmail = Fixture.Create<CreateEmailDto>();
            var postResponse = await PostAsync("/emails", expectedEmail);
            var insertedEmail = await Deserialize<EmailDto>(postResponse);

            // act
            var response = await Multipart(insertedEmail.Id, expectedAttachments);

            // assert
            response.IsSuccessStatusCode.Should().BeTrue();
            var getResponse = await GetAsync<EmailDto>($"/emails/{insertedEmail.Id}" );
            getResponse.Attachments
                .Should()
                .BeEquivalentTo(fileNames);
        }

        private Task<HttpResponseMessage> Multipart(Guid id, IEnumerable<IFormFile> files)
        {
            using var content = new MultipartFormDataContent();

            foreach (var file in files)
            {
                content.Add(new StreamContent(file.OpenReadStream()), "attachments", file.FileName);
            }

            return _client.PostAsync($"/emails/{id}/attachments", content);
        }

        private Task<HttpResponseMessage> PatchAsync<T>(string url, T dto)
        {
            return _client.PatchAsync(url, new StringContent(JsonSerializer.Serialize(dto, JsonSettings), 
                Encoding.UTF8, "application/json"));
        }

        private Task<HttpResponseMessage> PostAsync<T>(string url, T dto)
            => PostAsync(url, JsonSerializer.Serialize(dto, JsonSettings));

        private Task<HttpResponseMessage> PostAsync(string url, string payload)
        {
            return _client.PostAsync(url, new StringContent(payload, 
                Encoding.UTF8, "application/json"));
        }

        private async Task<T> GetAsync<T>(string url)
        {
            var stream = await _client.GetStreamAsync(url);
            return await JsonSerializer.DeserializeAsync<T>(stream, JsonSettings);
        }

        private static async Task<T> Deserialize<T>(HttpResponseMessage response)
        {
            var stream = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<T>(stream, JsonSettings);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _client.Dispose();
            _factory.Dispose();
        }
    }
}
