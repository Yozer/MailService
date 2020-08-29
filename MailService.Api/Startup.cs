using System.Text.Json;
using System.Text.Json.Serialization;
using Autofac;
using AutoMapper.Contrib.Autofac.DependencyInjection;
using FluentValidation.AspNetCore;
using MailService.Api.Infrastructure;
using MailService.Api.Infrastructure.Mongo;
using MailService.Api.Infrastructure.Smtp;
using MailService.Api.Model;
using MediatR.Extensions.Autofac.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MailService.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }
        public IHostEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                .AddJsonOptions(o =>
                {
                    o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    o.JsonSerializerOptions.WriteIndented = Environment.IsDevelopment();
                    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                })
                .AddFluentValidation(o => o.RegisterValidatorsFromAssembly(typeof(Program).Assembly));

            services.Configure<SmtpOptions>(Configuration.GetSection("smtp"));
            services.AddSwaggerGen();
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.AddMediatR(typeof(Program).Assembly);
            builder.AddAutoMapper(typeof(Program).Assembly);
            builder.AddMongo();

            builder.AddMongoRepository<EmailsRepository, EmailEntity>("emails");

            builder.RegisterType<SmtpClientWrapper>().AsImplementedInterfaces();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseMiddleware<ErrorHandlerMiddleware>();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Email Api V1");
            });
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
