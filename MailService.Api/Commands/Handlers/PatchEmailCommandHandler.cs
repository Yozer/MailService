using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MailService.Api.Dto;
using MailService.Api.Model;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MailService.Api.Commands.Handlers
{
    public class PatchEmailCommandHandler : IRequestHandler<PatchEmailCommand, bool>
    {
        private readonly IEmailsRepository _repository;
        private readonly ILogger<PatchEmailCommandHandler> _logger;

        public PatchEmailCommandHandler(IEmailsRepository repository, ILogger<PatchEmailCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<bool> Handle(PatchEmailCommand request, CancellationToken cancellationToken)
        {
            var patch = request.Patch;
            var email = await _repository.GetEmail(request.Id);
            if (email == null)
                return false;

            _logger.LogInformation($"Trying to updating email {email.Id}");
            await TryToUpdate(patch, email);

            return true;
        }

        private async Task TryToUpdate(CreateEmailDto patch, EmailEntity email)
        {
            bool updated = false;
            if (patch.Sender != null && patch.Sender != email.Sender)
            {
                email.UpdateSender(patch.Sender);
                updated = true;
            }

            if (patch.To != null && !patch.To.SequenceEqual(email.To))
            {
                email.UpdateTo(patch.To);
                updated = true;
            }

            if (patch.Body != null && patch.Body != email.Body)
            {
                email.UpdateBody(patch.Body);
                updated = true;
            }

            if (patch.Subject != null && patch.Subject != email.Subject)
            {
                email.UpdateSubject(patch.Subject);
                updated = true;
            }

            if (patch.Priority != null && patch.Priority.Value != email.Priority)
            {
                email.UpdatePriority(patch.Priority.Value);
                updated = true;
            }

            if (updated)
            {
                _logger.LogInformation($"Updated successfully {email.Id}");
                await _repository.UpdateEmail(email);
            }
            else
            {
                _logger.LogInformation($"Nothing to update for {email.Id}");
            }
        }
    }
}
