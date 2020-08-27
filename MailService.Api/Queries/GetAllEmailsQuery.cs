using System.Collections.Generic;
using MailService.Api.Dto;
using MediatR;

namespace MailService.Api.Queries
{
    public class GetAllEmailsQuery : IRequest<IEnumerable<EmailDto>>
    {
    }
}
