using MailService.Api.Dto;
using MediatR;

namespace MailService.Api.Commands
{
    public class CreateEmailCommand : IRequest<EmailDto>
    {
        public CreateEmailDto Email { get; }

        public CreateEmailCommand(CreateEmailDto email) 
            => Email = email;
    }
}
