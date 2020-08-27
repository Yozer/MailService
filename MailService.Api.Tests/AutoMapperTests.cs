using NUnit.Framework;

namespace MailService.Api.Tests
{
    public class AutoMapperTests : TestBase
    {
        [Test]
        public void ValidateAutoMapper()
        {
            var configuration = GetMapperConfig();
            configuration.AssertConfigurationIsValid();
        }
    }
}
