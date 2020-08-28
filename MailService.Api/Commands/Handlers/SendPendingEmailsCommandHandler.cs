using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace MailService.Api.Commands.Handlers
{
    public class SendPendingEmailsCommandHandler : AsyncRequestHandler<SendPendingEmailsCommand>
    {
        protected override Task Handle(SendPendingEmailsCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
