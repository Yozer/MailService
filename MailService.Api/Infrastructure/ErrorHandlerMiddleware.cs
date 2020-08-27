using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MailService.Api.Infrastructure
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHostEnvironment _environment;
        private readonly ILogger<ErrorHandlerMiddleware> _logger;

        public ErrorHandlerMiddleware(
            RequestDelegate next, 
            IHostEnvironment environment,
            ILogger<ErrorHandlerMiddleware> logger)
        {
            _next = next;
            _environment = environment;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException exception)
            {
                await HandleErrorAsync(context, exception);
            }
        }

        private Task HandleErrorAsync(HttpContext context, ValidationException exception)
        {
            var statusCode = HttpStatusCode.BadRequest;
            var response = new ApiResponse("Validation failed");
            response.Errors.AddRange(exception.Errors.Select(t => t.ToString()));

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            return JsonSerializer.SerializeAsync(context.Response.Body, response);
        }

        internal class ApiResponse
        {
            public string Message { get; set; }
            public List<string> Errors { get; set; } = new List<string>();

            public ApiResponse(string message)
            {
                Message = message;
            }
        }
    }
}
