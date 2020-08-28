using System;
using System.Collections.Generic;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace MailService.Api.Commands
{
    public class AddAttachmentsToEmailCommand : IRequest<bool>
    {
        public Guid Id { get; }
        public ICollection<IFormFile> Attachments { get; }

        public AddAttachmentsToEmailCommand(Guid id, ICollection<IFormFile> attachments)
        {
            Id = id;
            Attachments = attachments;
        }
    }
}
