using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MailService.Api.Infrastructure.Mongo;

namespace MailService.Api.Model
{
    public interface IEmailsRepository : IMongoRepository<EmailEntity>
    {
        IAsyncEnumerable<EmailEntity> GetEmails(EmailStatus status);
        IAsyncEnumerable<EmailEntity> GetAllEmails();
        Task<EmailEntity> GetEmail(Guid id);
        Task AddNewEmail(EmailEntity emailEntity);
        Task UpdateEmail(EmailEntity emailEntity);
    }
}