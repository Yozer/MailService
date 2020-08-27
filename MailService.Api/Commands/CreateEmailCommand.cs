using MailService.Api.Dto;
using MediatR;

namespace MailService.Api.Commands
{
    public class CreateEmailCommand : IRequest<EmailDto>
    {
        public EmailDto Email { get; }

        public CreateEmailCommand(EmailDto email) 
            => Email = email;
    }
}
