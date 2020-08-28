using System;
using System.Threading;
using System.Threading.Tasks;
using MailService.Api.Dto;
using MediatR;

namespace MailService.Api.Commands.Handlers
{
    public class CreateEmailCommandHandler : IRequestHandler<CreateEmailCommand, EmailDto>
    {
        public Task<EmailDto> Handle(CreateEmailCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
