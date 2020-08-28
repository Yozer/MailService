using System.Linq;
using AutoMapper;
using MailService.Api.Dto;
using MailService.Api.Model;

namespace MailService.Api.Infrastructure
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<EmailEntity, EmailDto>()
                .ForMember(t => t.Attachments, opt =>
                {
                    opt.MapFrom(t => t.Attachments.Select(x => x.Name));
                });
        }
    }
}
