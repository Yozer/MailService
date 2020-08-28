using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using MailService.Api.Infrastructure.Smtp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using NUnit.Framework;

namespace MailService.Api.Tests.Integration
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Startup>
    {
        public ISmtpClient SmtpClient { get; } = Substitute.For<ISmtpClient>();
        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.UseServiceProviderFactory(new CustomServiceProviderFactory());
            return base.CreateHost(builder);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder
                .ConfigureAppConfiguration((context, configBuilder) =>
                {
                    configBuilder
                        .SetBasePath(TestContext.CurrentContext.TestDirectory);
                })
                .ConfigureTestContainer<ContainerBuilder>(container =>
                {
                    // called after Startup.ConfigureContainer
                    container.Register(o => SmtpClient);
                })
                .UseEnvironment("IntegrationTests");
        }
    }

    /// <summary>
    /// Based upon https://github.com/dotnet/aspnetcore/issues/14907#issuecomment-620750841 - only necessary because of an issue in ASP.NET Core
    /// </summary>
    public class CustomServiceProviderFactory : IServiceProviderFactory<CustomContainerBuilder> {
        public CustomContainerBuilder CreateBuilder(IServiceCollection services) => new CustomContainerBuilder(services);

        public IServiceProvider CreateServiceProvider(CustomContainerBuilder containerBuilder) =>
            new AutofacServiceProvider(containerBuilder.CustomBuild());
    }

    public class CustomContainerBuilder : ContainerBuilder
    {
        private readonly IServiceCollection _services;

        public CustomContainerBuilder(IServiceCollection services)
        {
            this._services = services;
            this.Populate(services);
        }

        public IContainer CustomBuild()
        {
            var sp = this._services.BuildServiceProvider();
#pragma warning disable CS0612 // Type or member is obsolete
            var filters =
                sp.GetRequiredService<IEnumerable<IStartupConfigureContainerFilter<Autofac.ContainerBuilder>>>();
#pragma warning restore CS0612 // Type or member is obsolete

            foreach (var filter in filters)
            {
                filter.ConfigureContainer(b => { })(this);
            }

            return this.Build();
        }
    }
}