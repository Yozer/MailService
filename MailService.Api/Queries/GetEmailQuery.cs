using System;
using MailService.Api.Dto;
using MediatR;

namespace MailService.Api.Queries
{
    public class GetEmailQuery : IRequest<EmailDto>
    {
        public Guid Id { get; }

        public GetEmailQuery(Guid id)
        {
            Id = id;
        }
    }
}
