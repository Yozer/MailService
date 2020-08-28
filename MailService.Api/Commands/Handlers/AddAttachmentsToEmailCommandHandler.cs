using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace MailService.Api.Commands.Handlers
{
    public class AddAttachmentsToEmailCommandHandler : AsyncRequestHandler<AddAttachmentsToEmailCommand>
    {
        protected override Task Handle(AddAttachmentsToEmailCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
