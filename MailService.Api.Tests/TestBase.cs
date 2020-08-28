using System;
using System.Linq;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Kernel;
using AutoMapper;
using MailService.Api.Dto;
using MailService.Api.Infrastructure;
using MailService.Api.Model;
using NUnit.Framework;

namespace MailService.Api.Tests
{
    public abstract class TestBase
    {
        public IFixture Fixture { get; private set; }

        [SetUp]
        public void BaseSetup()
        {
            Fixture = new Fixture()
                .Customize(new AutoNSubstituteCustomization());
            Fixture.Customizations.Add(new EmailEntityGenerator());
            Fixture.Customizations.Add(new CreateEmailDtoGenerator());

            Fixture.Inject<IMapper>(new Mapper(GetMapperConfig()));
        }

        protected T Freeze<T>()
            => Fixture.Freeze<T>();

        protected T Create<T>()
            => Fixture.Create<T>();

        protected IConfigurationProvider GetMapperConfig()
        {
            var profile = new AutoMapperProfile();
            var configuration = new MapperConfiguration(t => t.AddProfile(profile));
            return configuration;
        }
    }

    public class EmailEntityGenerator : ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext context)
        {
            var type = request as Type;

            if (type == null)
                return new NoSpecimen();

            if (type != typeof(EmailEntity))
                return new NoSpecimen();

            return new EmailEntity(
                context.Create<string>(), 
                context.Create<string>(), 
                "from@gmail.com", new[] {"first@wp.pl", "second@wp.pl"},
                context.Create<EmailPriority>());
        }
    }

    public class CreateEmailDtoGenerator : ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext context)
        {
            var type = request as Type;

            if (type == null)
                return new NoSpecimen();

            if (type != typeof(CreateEmailDto))
                return new NoSpecimen();

            return new CreateEmailDto
            {
                To = new[] {"first@test.pl", "second@test.pl"}.ToList(),
                Sender = "from@onet.eu",
                Body = context.Create<string>(),
                Subject = context.Create<string>(),
                Priority = context.Create<EmailPriority>()
            };
        }
    }
}