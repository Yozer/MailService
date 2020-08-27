using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MailService.Api.Dto;
using MailService.Api.Model;
using MediatR;

namespace MailService.Api.Queries.Handlers
{
    public class GetEmailQueryHandler : IRequestHandler<GetEmailQuery, EmailDto>
    {
        private readonly IEmailsRepository _repository;
        private readonly IMapper _mapper;

        public GetEmailQueryHandler(IEmailsRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<EmailDto> Handle(GetEmailQuery request, CancellationToken cancellationToken)
        {
            var email = await _repository.GetEmail(request.Id);
            return _mapper.Map<EmailDto>(email);
        }
    }
}