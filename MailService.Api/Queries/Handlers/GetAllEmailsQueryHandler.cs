using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MailService.Api.Dto;
using MailService.Api.Model;
using MediatR;

namespace MailService.Api.Queries.Handlers
{
    public class GetAllEmailsQueryHandler : IRequestHandler<GetAllEmailsQuery, IEnumerable<EmailDto>>
    {
        private readonly IEmailsRepository _repository;
        private readonly IMapper _mapper;

        public GetAllEmailsQueryHandler(IEmailsRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<EmailDto>> Handle(GetAllEmailsQuery request, CancellationToken cancellationToken)
        {
            var emails = await _repository.GetAllEmails().ToListAsync(cancellationToken);
            return _mapper.Map<IEnumerable<EmailDto>>(emails);
        }
    }
}
