using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MailService.Api.Model;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MailService.Api.Commands.Handlers
{
    public class AddAttachmentsToEmailCommandHandler : IRequestHandler<AddAttachmentsToEmailCommand, bool>
    {
        private readonly IEmailsRepository _repository;
        private readonly ILogger<AddAttachmentsToEmailCommandHandler> _logger;

        public AddAttachmentsToEmailCommandHandler(IEmailsRepository repository, ILogger<AddAttachmentsToEmailCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<bool> Handle(AddAttachmentsToEmailCommand request, CancellationToken cancellationToken)
        {
            var email = await _repository.GetEmail(request.Id);
            if (email == null)
                return false;

            _logger.LogInformation($"Adding {request.Attachments.Count} attachments to email {request.Id}");
            foreach (var file in request.Attachments)
            {
                await using var stream = file.OpenReadStream();
                var data = await ReadFully(stream);
                email.AddAttachment(file.FileName, data);

                _logger.LogInformation($"Added {file.FileName} with length {file.Length} to email {request.Id}");
            }

            await _repository.UpdateEmail(email);
            _logger.LogInformation($"Saved {request.Attachments.Count} attachments for email {request.Id}");
            return true;
        }

        public static async Task<byte[]> ReadFully(Stream input)
        {
            await using var ms = new MemoryStream();
            await input.CopyToAsync(ms);
            return ms.ToArray();
        }
    }
}