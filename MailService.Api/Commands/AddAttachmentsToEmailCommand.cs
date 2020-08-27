using System.Collections.Generic;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace MailService.Api.Commands
{
    public class AddAttachmentsToEmailCommand : IRequest
    {
        public string Id { get; }
        public ICollection<IFormFile> Attachments { get; }

        public AddAttachmentsToEmailCommand(string id, ICollection<IFormFile> attachments)
        {
            Id = id;
            Attachments = attachments;
        }
    }
}
