using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using MailService.Api.Infrastructure;
using NUnit.Framework;

namespace MailService.Api.Tests
{
    public class AutoMapperTests
    {
        [Test]
        public void ValidateAutoMapper()
        {
            var profile = new AutoMapperProfile();
            var configuration = new MapperConfiguration(t => t.AddProfile(profile));
            configuration.AssertConfigurationIsValid();
        }
    }
}
