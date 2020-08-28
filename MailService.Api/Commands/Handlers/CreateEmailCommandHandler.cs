using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MailService.Api.Dto;
using MailService.Api.Model;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MailService.Api.Commands.Handlers
{
    public class CreateEmailCommandHandler : IRequestHandler<CreateEmailCommand, EmailDto>
    {
        private readonly IMapper _mapper;
        private readonly IEmailsRepository _repository;
        private readonly ILogger<CreateEmailCommandHandler> _logger;

        public CreateEmailCommandHandler(IMapper mapper, IEmailsRepository repository, ILogger<CreateEmailCommandHandler> logger)
        {
            _mapper = mapper;
            _repository = repository;
            _logger = logger;
        }

        public async Task<EmailDto> Handle(CreateEmailCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Email;
            var priority = dto.Priority ?? EmailPriority.Normal;
            var email = new EmailEntity(dto.Subject, dto.Body, dto.Sender, dto.To, priority);
            await _repository.AddNewEmail(email);

            _logger.LogInformation($"Saved new email {email.Id}");
            return _mapper.Map<EmailDto>(email);
        }
    }
}