using AutoFixture;
using AutoFixture.AutoNSubstitute;
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

        }

        public T Freeze<T>()
            => Fixture.Freeze<T>();

        public T Create<T>()
            => Fixture.Create<T>();
    }
}