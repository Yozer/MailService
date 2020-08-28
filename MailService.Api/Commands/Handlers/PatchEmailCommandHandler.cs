using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace MailService.Api.Commands.Handlers
{
    public class PatchEmailCommandHandler : IRequestHandler<PatchEmailCommand, bool>
    {
        public Task<bool> Handle(PatchEmailCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
